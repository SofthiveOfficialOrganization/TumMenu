using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
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

builder.Services
    .AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<AppExceptionFilter>();
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/status-code/{0}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.SeedAdminAsync();
await app.RunAsync();

public partial class Program { }
