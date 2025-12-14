using Microsoft.AspNetCore.Identity.UI.Services;
using WebUI.Extensions;
using WebUI.ExternalServices;
using WebUI.Filters;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.AddServices();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/Login";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services
	.AddControllersWithViews(options =>
	{
		options.Filters.Add<AppExceptionFilter>();
		options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

	});
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("Owner", p =>
	{
		p.RequireRole("Owner");
		p.RequireClaim("owner_id");
	})
	.AddPolicy("Admin", p => p.RequireRole("Admin"));

builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if(app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?code={0}");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.SeedAdminAsync();
await app.RunAsync();
