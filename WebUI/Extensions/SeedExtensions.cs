using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Extensions;

public static class SeedExtensions
{
    public static async Task SeedAdminAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedBlogPostsAsync(db);

        string adminEmail = "yonetim@softhive.com";
        string adminPassword = "SoftHive123!";
        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new ApplicationRole(adminRole));
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            await userManager.AddToRoleAsync(admin, adminRole);
        }
    }

    private static async Task SeedBlogPostsAsync(ApplicationDbContext db)
    {
        var existingSlugs = await db.BlogPosts
            .Select(b => b.Slug)
            .ToListAsync();

        var toAdd = BlogPostSeedData.GetPosts()
            .Where(p => !existingSlugs.Contains(p.Slug, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (toAdd.Count > 0)
        {
            db.BlogPosts.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }
}
