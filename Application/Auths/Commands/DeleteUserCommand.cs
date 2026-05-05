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
        var user = await userManager.FindByIdAsync(request.Id);

        if (user == null) return false;

        var ownerId = await context.Owners
            .IgnoreQueryFilters()
            .Where(o => o.ApplicationUserId == user.Id)
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerId != null)
        {
            var companyId = await context.Companies
                .IgnoreQueryFilters()
                .Where(c => c.OwnerId == ownerId.Value)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (companyId != null)
            {
                await DeleteCompanyGraphAsync(companyId.Value, cancellationToken);
            }

            await context.OwnerIssueReports
                .IgnoreQueryFilters()
                .Where(r => r.OwnerId == ownerId.Value)
                .ExecuteDeleteAsync(cancellationToken);

            await context.Owners
                .IgnoreQueryFilters()
                .Where(o => o.Id == ownerId.Value)
                .ExecuteDeleteAsync(cancellationToken);
        }

        await context.AuditLogs
            .IgnoreQueryFilters()
            .Where(l => l.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.ToUserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private async Task DeleteCompanyGraphAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var storeIds = await context.Stores
            .IgnoreQueryFilters()
            .Where(s => s.CompanyId == companyId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var menuIds = await context.Menus
            .IgnoreQueryFilters()
            .Where(m => m.CompanyId == companyId || (m.StoreId != null && storeIds.Contains(m.StoreId.Value)))
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var categoryIds = await context.Categories
            .IgnoreQueryFilters()
            .Where(c => menuIds.Contains(c.MenuId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var productIds = await context.Products
            .IgnoreQueryFilters()
            .Where(p => categoryIds.Contains(p.CategoryId))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var qrCodeIds = await context.QRCodes
            .IgnoreQueryFilters()
            .Where(q => q.StoreId != null && storeIds.Contains(q.StoreId.Value))
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        await context.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(c => c.DefaultMainMenuId, (Guid?)null)
                    .SetProperty(c => c.DefaultPaymentMethodId, (Guid?)null),
                cancellationToken);

        await context.Medias
            .IgnoreQueryFilters()
            .Where(m =>
                m.CompanyId == companyId ||
                (m.StoreId != null && storeIds.Contains(m.StoreId.Value)) ||
                (m.MenuId != null && menuIds.Contains(m.MenuId.Value)) ||
                (m.ProductId != null && productIds.Contains(m.ProductId.Value)) ||
                (m.Type == MediaRefType.Company && m.ReferenceId == companyId) ||
                (m.Type == MediaRefType.Store && storeIds.Contains(m.ReferenceId)) ||
                (m.Type == MediaRefType.Menu && menuIds.Contains(m.ReferenceId)) ||
                (m.Type == MediaRefType.Category && categoryIds.Contains(m.ReferenceId)) ||
                (m.Type == MediaRefType.Product && productIds.Contains(m.ReferenceId)) ||
                (m.Type == MediaRefType.QRCode && qrCodeIds.Contains(m.ReferenceId)))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Notifications
            .IgnoreQueryFilters()
            .Where(n => n.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Categories
            .IgnoreQueryFilters()
            .Where(c => categoryIds.Contains(c.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.ParentId, (Guid?)null),
                cancellationToken);

        await context.Products
            .IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Categories
            .IgnoreQueryFilters()
            .Where(c => categoryIds.Contains(c.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.QRCodes
            .IgnoreQueryFilters()
            .Where(q => qrCodeIds.Contains(q.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Menus
            .IgnoreQueryFilters()
            .Where(m => menuIds.Contains(m.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Staffs
            .IgnoreQueryFilters()
            .Where(s => storeIds.Contains(s.StoreId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Stores
            .IgnoreQueryFilters()
            .Where(s => storeIds.Contains(s.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
