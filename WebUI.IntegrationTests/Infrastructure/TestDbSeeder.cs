using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Infrastructure;

public class TestDbSeeder
{
    public const string AdminEmail = "test-admin@tummenu.com";
    public const string AdminPassword = "TestAdmin123!";
    public const string OwnerEmail = "test-owner@tummenu.com";
    public const string OwnerPassword = "TestOwner123!";

    public static Guid OwnerId { get; private set; }
    public static Guid CompanyId { get; private set; }
    public static Guid StoreId { get; private set; }
    public static Guid MenuId { get; private set; }
    public static string OwnerUserId { get; private set; } = string.Empty;
    public static string AdminUserId { get; private set; } = string.Empty;

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();

        // Ensure roles exist
        foreach (var role in new[] { "Admin", "Owner" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        // Admin user
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, AdminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        AdminUserId = adminUser.Id;

        // Owner user
        var ownerUser = await userManager.FindByEmailAsync(OwnerEmail);
        if (ownerUser == null)
        {
            ownerUser = new ApplicationUser { UserName = OwnerEmail, Email = OwnerEmail, EmailConfirmed = true };
            await userManager.CreateAsync(ownerUser, OwnerPassword);
            await userManager.AddToRoleAsync(ownerUser, "Owner");
        }
        OwnerUserId = ownerUser.Id;

        // Owner record
        var owner = db.Owners.FirstOrDefault(o => o.ApplicationUserId == ownerUser.Id);
        if (owner == null)
        {
            owner = new Owner { ApplicationUserId = ownerUser.Id, WizardCompleted = true };
            db.Owners.Add(owner);
            await db.SaveChangesAsync();
        }
        OwnerId = owner.Id;

        // Ensure wizard is marked complete so EnsureCompanyExistsFilter doesn't redirect
        if (!owner.WizardCompleted)
        {
            owner.WizardCompleted = true;
            await db.SaveChangesAsync();
        }

        // Add owner claim to user
        var claims = await userManager.GetClaimsAsync(ownerUser);
        if (!claims.Any(c => c.Type == "owner_id"))
            await userManager.AddClaimAsync(ownerUser, new System.Security.Claims.Claim("owner_id", owner.Id.ToString()));

        // Company
        var company = db.Companies.FirstOrDefault(c => c.OwnerId == owner.Id);
        if (company == null)
        {
            company = new Company { Title = "Test Şirketi", Slug = "test-sirketi", OwnerId = owner.Id };
            db.Companies.Add(company);
            await db.SaveChangesAsync();
        }
        CompanyId = company.Id;

        // Store
        var store = db.Stores.FirstOrDefault(s => s.CompanyId == company.Id);
        if (store == null)
        {
            store = new Store { Title = "Test Şubesi", Slug = "test-subesi", CompanyId = company.Id, PhoneNumber = "05001234567" };
            db.Stores.Add(store);
            await db.SaveChangesAsync();
        }
        StoreId = store.Id;

        // Menu
        var menu = db.Menus.FirstOrDefault(m => m.StoreId == store.Id);
        if (menu == null)
        {
            menu = new Menu { Title = "Test Menü", StoreId = store.Id, Status = MenuStatus.Active };
            db.Menus.Add(menu);
            await db.SaveChangesAsync();
        }
        MenuId = menu.Id;
    }

    public static async Task CleanupAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var menu = await db.Menus.FindAsync(MenuId);
        if (menu != null) db.Menus.Remove(menu);

        var store = await db.Stores.FindAsync(StoreId);
        if (store != null) db.Stores.Remove(store);

        var company = await db.Companies.FindAsync(CompanyId);
        if (company != null) db.Companies.Remove(company);

        var owner = await db.Owners.FindAsync(OwnerId);
        if (owner != null) db.Owners.Remove(owner);

        await db.SaveChangesAsync();

        var ownerUser = await userManager.FindByEmailAsync(OwnerEmail);
        if (ownerUser != null) await userManager.DeleteAsync(ownerUser);

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser != null) await userManager.DeleteAsync(adminUser);
    }
}
