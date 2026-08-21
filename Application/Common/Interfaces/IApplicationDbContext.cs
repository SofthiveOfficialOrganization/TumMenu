using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<Owner> Owners { get; }
    DbSet<Company> Companies { get; }
    DbSet<Store> Stores { get; }
    DbSet<StoreSocialLink> StoreSocialLinks { get; }
    DbSet<Address> Addresses { get; }
    DbSet<QRCode> QRCodes { get; }
    DbSet<Menu> Menus { get; }
    DbSet<MenuDesign> MenuDesigns { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<Media> Medias { get; }
    DbSet<Staff> Staffs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<OwnerIssueReport> OwnerIssueReports { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<SystemLog> SystemLogs { get; }
    DbSet<QrOrderSession> QrOrderSessions { get; }
    DbSet<CustomerOrderRequest> CustomerOrderRequests { get; }
    DbSet<CustomerOrderRequestItem> CustomerOrderRequestItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
