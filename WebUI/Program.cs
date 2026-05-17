using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WebUI.Contracts;
using WebUI.Extensions;
using WebUI.ExternalServices;
using WebUI.Filters;
using WebUI.Middleware;
using WebUI.Security;

var builder = WebApplication.CreateBuilder(args);
var seedOnly = args.Any(arg => string.Equals(arg, "--seed-only", StringComparison.OrdinalIgnoreCase));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.AddServices();

var dataProtectionKeysDirectory = new DirectoryInfo(
	Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys"));

builder.Services
	.AddDataProtection()
	.PersistKeysToFileSystem(dataProtectionKeysDirectory)
	.SetApplicationName("TumMenu.WebUI");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor |
		ForwardedHeaders.XForwardedProto |
		ForwardedHeaders.XForwardedHost;

	// We are behind a reverse proxy (IIS/Cloudflare). Trust forwarded headers.
	// Actual network restrictions should be enforced at the infrastructure layer.
	options.KnownNetworks.Clear();
	options.KnownProxies.Clear();
});

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/giris";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.ReturnUrlParameter = "DonusUrl";
	options.ExpireTimeSpan = TimeSpan.FromHours(2);
	options.SlidingExpiration = true;
	options.Cookie.Name = ".TumMenu.Auth";
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
		? CookieSecurePolicy.SameAsRequest
		: CookieSecurePolicy.Always;

	options.Events.OnRedirectToLogin = context =>
	{
		if (IsApiRequest(context.Request))
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		}

		var tempDataFactory = context.HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
		if (tempDataFactory != null)
		{
			var tempData = tempDataFactory.GetTempData(context.HttpContext);
			tempData["SessionExpired"] = "Oturum süreniz sona erdi. Lütfen tekrar giriş yapın.";
		}

		context.Response.Redirect(options.LoginPath);
		return Task.CompletedTask;
	};
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("OwnerOnly", p =>
	{
		p.RequireRole("Owner");
		p.RequireClaim("owner_id");
	})
	.AddPolicy("AdminOnly", p =>
	{
		p.RequireRole("Admin");
	})
	.AddPolicy("OwnerOrAdmin", p =>
	{
		p.RequireAssertion(ctx =>
			ctx.User.IsInRole("Admin") ||
			(ctx.User.IsInRole("Owner") && ctx.User.HasClaim(c => c.Type == "owner_id"))
		);
	});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
	.AddMvcOptions(options =>
	{
		options.Filters.Add<AppExceptionFilter>();
		options.Filters.Add<ValidationLoggingFilter>();
		options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
		options.Filters.Add<EnsureCompanyExistsFilter>();
		options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
	});
builder.Services.AddHealthChecks();



var app = builder.Build();
var serviceProvider = app.Services.CreateScope().ServiceProvider;
var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();

if(app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

var supportedCultures = new[] { "tr-TR" };
var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture(supportedCultures[0])
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseForwardedHeaders();

// Canonical host: http(s)://*.tummenu.com and http://tummenu.com -> https://tummenu.com
// app.Use(async (context, next) =>
// {
// 	if (!app.Environment.IsDevelopment())
// 	{
// 		var request = context.Request;
// 		var host = request.Host.Host.ToLowerInvariant();
// 		var isApexHost = string.Equals(host, "tummenu.com", StringComparison.Ordinal);
// 		var isSubdomainHost = host.EndsWith(".tummenu.com", StringComparison.Ordinal);

// 		if (isApexHost || isSubdomainHost)
// 		{
// 			var needsHttps = !request.IsHttps;
// 			var needsCanonicalHost = !isApexHost;

// 			if (needsHttps || needsCanonicalHost)
// 			{
// 				var canonicalUrl = $"https://tummenu.com{request.PathBase}{request.Path}{request.QueryString}";
// 				context.Response.Redirect(canonicalUrl, permanent: true);
// 				return;
// 			}
// 		}
// 	}

// 	await next();
// });

app.Use(async (context, next) =>
{
	context.Response.OnStarting(() =>
	{
		var headers = context.Response.Headers;

		headers.TryAdd("X-Content-Type-Options", "nosniff");
		headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
		headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), payment=(), usb=(), geolocation=(self)");

		if (!headers.ContainsKey("Content-Security-Policy"))
		{
			headers.ContentSecurityPolicy = string.Join("; ", new[]
			{
				"default-src 'self'",
				"base-uri 'self'",
				"object-src 'none'",
				"frame-ancestors 'self'",
				"form-action 'self'",
				"img-src 'self' data: blob: https:",
				"font-src 'self' data: https://cdn.jsdelivr.net https://unpkg.com https://cdnjs.cloudflare.com https://fonts.gstatic.com",
				"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com https://cdnjs.cloudflare.com https://fonts.googleapis.com",
				"script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://code.jquery.com https://unpkg.com https://pagead2.googlesyndication.com https://www.googletagmanager.com https://www.google-analytics.com https://googleads.g.doubleclick.net https://tpc.googlesyndication.com https://ep2.adtrafficquality.google https://static.cloudflareinsights.com https://challenges.cloudflare.com",
				"connect-src 'self' https: wss:",
				"frame-src 'self' https://googleads.g.doubleclick.net https://tpc.googlesyndication.com https://www.google.com https://ep2.adtrafficquality.google https://challenges.cloudflare.com",
				"worker-src 'self' blob:",
				"media-src 'self'",
				"upgrade-insecure-requests"
			});
		}

		return Task.CompletedTask;
	});

	await next();
});

// Prevent cached/stale antiforgery tokens on auth-related form pages.
app.Use(async (context, next) =>
{
	var path = context.Request.Path;
	var isAuthFormPage =
		path.Equals("/giris", StringComparison.OrdinalIgnoreCase) ||
		path.Equals("/kayit", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/Identity/Account", StringComparison.OrdinalIgnoreCase);

	if (isAuthFormPage)
	{
		context.Response.OnStarting(() =>
		{
			context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
			context.Response.Headers.Pragma = "no-cache";
			context.Response.Headers.Expires = "0";
			return Task.CompletedTask;
		});
	}

	await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		var path = ctx.Context.Request.Path.Value ?? string.Empty;
		if (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
		{
			ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
		}
		else if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
		{
			ctx.Context.Response.Headers.CacheControl = "public, max-age=2592000";
		}
	}
});

app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    var noIndexPrefixes = new[]
    {
        "/Admin",
        "/Identity",
        "/api",
        "/user",
        "/owner",
        "/error",
        "/status-code"
    };
    var noIndexExactPaths = new[]
    {
        "/health",
        "/giris",
        "/kayit",
        "/guncelleniyor",
        "/hesap-aktivasyon",
        "/Account/AccountActivationSuccess"
    };

    if (noIndexPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        || noIndexExactPaths.Any(noIndexPath => string.Equals(path, noIndexPath, StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/status-code/{0}");
app.UseMiddleware<SecurityRequestLoggingMiddleware>();

app.Use(async (context, next) =>
{
	if (SecurityRequestClassifier.IsKnownSecurityProbe(context.Request.Path))
	{
		context.Response.StatusCode = StatusCodes.Status404NotFound;
		return;
	}

	await next();
});

app.MapHealthChecks("/health").AllowAnonymous();

app.Map("/api", ApiNotFound).AllowAnonymous();
app.Map("/api/{**path}", ApiNotFound).AllowAnonymous();
app.Map("/owner/{**path}", ApiNotFound).AllowAnonymous();
app.Map("/user/{**path}", ApiNotFound).AllowAnonymous();

app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "publicProduct",
	pattern: "{companySlug:regex(^[a-z0-9][a-z0-9-]*$)}/{storeSlug:regex(^[a-z0-9][a-z0-9-]*$)}/{categorySlug:regex(^[a-z0-9][a-z0-9-]*$)}/{productSlug:regex(^[a-z0-9][a-z0-9-]*$)}",
	defaults: new { controller = "Menu", action = "Product" });

app.MapControllerRoute(
	name: "publicStoreLanding",
	pattern: "{companySlug:regex(^[a-z0-9][a-z0-9-]*$)}/{storeSlug:regex(^[a-z0-9][a-z0-9-]*$)}/magaza",
	defaults: new { controller = "Store", action = "Public" });

app.MapControllerRoute(
	name: "publicCategory",
	pattern: "{companySlug:regex(^[a-z0-9][a-z0-9-]*$)}/{storeSlug:regex(^[a-z0-9][a-z0-9-]*$)}/{categorySlug:regex(^[a-z0-9][a-z0-9-]*$)}",
	defaults: new { controller = "Menu", action = "Category" });

app.MapControllerRoute(
	name: "publicStore",
	pattern: "{companySlug:regex(^[a-z0-9][a-z0-9-]*$)}/{storeSlug:regex(^[a-z0-9][a-z0-9-]*$)}",
	defaults: new { controller = "Menu", action = "Index" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


await app.SeedAdminAsync();

if (seedOnly)
{
	return;
}

await app.RunAsync();

static bool IsApiRequest(HttpRequest request)
{
	return request.Path.StartsWithSegments("/api")
		|| request.Path.StartsWithSegments("/user")
		|| request.Path.StartsWithSegments("/owner")
		|| string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}

static IResult ApiNotFound(HttpContext context)
{
	var payload = new ApiError
	{
		Status = StatusCodes.Status404NotFound,
		Code = "not_found",
		Message = "Bulunamadı",
		TraceId = context.TraceIdentifier
	};

	return Results.Json(payload, statusCode: payload.Status);
}

public partial class Program { }
