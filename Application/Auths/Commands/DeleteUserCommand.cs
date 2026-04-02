using Application.Abstractions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Commands;

public class DeleteUserCommand : IRequest<bool>, IAuditableCommand
{
    public string Id { get; set; } = null!;
    public string ActionName => "Kullanıcı silindi";
}

public class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext context
) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
                    .ThenInclude(c => c!.Stores)
                        .ThenInclude(s => s.Address)
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
                    .ThenInclude(c => c!.Stores)
                        .ThenInclude(s => s.Staffs)
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
                    .ThenInclude(c => c!.Stores)
                        .ThenInclude(s => s.QRCode)
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
                    .ThenInclude(c => c!.Stores)
                        .ThenInclude(s => s.Menus)
                            .ThenInclude(m => m.Categories)
                                .ThenInclude(cat => cat.Products)
            .Include(u => u.Owner)
                .ThenInclude(o => o!.Company)
                    .ThenInclude(c => c!.Stores)
                        .ThenInclude(s => s.Medias)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null) return false;

        if (user.Owner?.Company != null)
        {
            var company = user.Owner.Company;
            var storeIds = company.Stores.Select(s => s.Id).ToList();

            // Store'ları ve bağlı verileri sil (Menüler hariç, onları toplu sileceğiz)
            foreach (var store in company.Stores)
            {
                // Store'a bağlı medyaları sil
                var storeMedias = await context.Medias
                    .Where(m => m.StoreId == store.Id || (m.ReferenceId == store.Id && m.Type == MediaRefType.Store))
                    .ToListAsync(cancellationToken);
                context.Medias.RemoveRange(storeMedias);

                // Store'a bağlı QRCode'u sil
                if (store.QRCode != null)
                {
                    var qrMedias = await context.Medias
                        .Where(m => m.ReferenceId == store.QRCode.Id && m.Type == MediaRefType.QRCode)
                        .ToListAsync(cancellationToken);
                    context.Medias.RemoveRange(qrMedias);
                    context.QRCodes.Remove(store.QRCode);
                }

                // Store'a bağlı adresi sil
                if (store.Address != null)
                {
                    context.Addresses.Remove(store.Address);
                }

                // Store'a bağlı staff'ları sil
                context.Staffs.RemoveRange(store.Staffs);

                context.Stores.Remove(store);
            }

            // Şirkete veya Store'lara bağlı TÜM menüleri bul
            var allMenus = await context.Menus
                .Include(m => m.Categories)
                    .ThenInclude(c => c.Products)
                .Where(m => m.CompanyId == company.Id || (m.StoreId != null && storeIds.Contains(m.StoreId.Value)))
                .ToListAsync(cancellationToken);

            foreach (var menu in allMenus)
            {
                // Menü medyalarını sil
                var menuMedias = await context.Medias
                    .Where(m => m.MenuId == menu.Id || (m.ReferenceId == menu.Id && m.Type == MediaRefType.Menu))
                    .ToListAsync(cancellationToken);
                context.Medias.RemoveRange(menuMedias);

                // Kategorileri ve ürünleri sil
                // NOT: hierarchical categories için MenuId üzerinden tüm kategorileri çekmek daha sağlıklı olabilir
                var allCategories = await context.Categories
                    .Include(c => c.Products)
                    .Where(c => c.MenuId == menu.Id)
                    .ToListAsync(cancellationToken);

                foreach (var category in allCategories)
                {
                    // Kategori medyalarını sil
                    var categoryMedias = await context.Medias
                        .Where(m => m.ReferenceId == category.Id && m.Type == MediaRefType.Category)
                        .ToListAsync(cancellationToken);
                    context.Medias.RemoveRange(categoryMedias);

                    // Ürünleri ve ürün medyalarını sil
                    foreach (var product in category.Products)
                    {
                        var productMedias = await context.Medias
                            .Where(m => m.ProductId == product.Id || (m.ReferenceId == product.Id && m.Type == MediaRefType.Product))
                            .ToListAsync(cancellationToken);
                        context.Medias.RemoveRange(productMedias);
                        context.Products.Remove(product);
                    }

                    context.Categories.Remove(category);
                }

                context.Menus.Remove(menu);
            }

            // Company medyalarını sil
            var companyMedias = await context.Medias
                .Where(m => m.CompanyId == company.Id || (m.ReferenceId == company.Id && m.Type == MediaRefType.Company))
                .ToListAsync(cancellationToken);
            context.Medias.RemoveRange(companyMedias);

            context.Companies.Remove(company);
        }

        // Audit logları ve bildirimleri sil
        var logs = await context.AuditLogs.Where(l => l.UserId == user.Id).ToListAsync(cancellationToken);
        context.AuditLogs.RemoveRange(logs);

        var notifications = await context.Notifications.Where(n => n.ToUserId == user.Id).ToListAsync(cancellationToken);
        context.Notifications.RemoveRange(notifications);

        // Owner'ı sil
        if (user.Owner != null)
        {
            context.Owners.Remove(user.Owner);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Son olarak kullanıcıyı sil
        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
