using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.CustomerOrderRequests.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Queries;

public sealed class GetAdminOrderRequestsQuery : IRequest<List<CustomerOrderRequestSummaryDTO>>, IAuthorizedRequest
{
    public CustomerOrderRequestStatus? Status { get; set; }
    public Guid? StoreId { get; set; }
    public DateTime? Date { get; set; }
}

public sealed class GetAdminOrderRequestsQueryHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : IRequestHandler<GetAdminOrderRequestsQuery, List<CustomerOrderRequestSummaryDTO>>
{
    public async Task<List<CustomerOrderRequestSummaryDTO>> Handle(GetAdminOrderRequestsQuery request, CancellationToken ct)
    {
        var companyId = await ResolveCompanyIdAsync(ct);
        var query = db.CustomerOrderRequests
            .Include(x => x.Store)
            .AsNoTracking()
            .AsQueryable();

        if (!userContext.IsAdmin)
        {
            query = query.Where(x => x.CompanyId == companyId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.StoreId.HasValue)
        {
            query = query.Where(x => x.StoreId == request.StoreId.Value);
        }

        if (request.Date.HasValue)
        {
            var start = new DateTimeOffset(request.Date.Value.Date, TimeSpan.FromHours(3));
            var end = start.AddDays(1);
            query = query.Where(x => x.CreatedAt >= start && x.CreatedAt < end);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new CustomerOrderRequestSummaryDTO
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                StoreId = x.StoreId,
                StoreName = x.Store.Title,
                CustomerName = x.CustomerName,
                TableNumber = x.TableNumber,
                Status = x.Status,
                Subtotal = x.Subtotal,
                ItemCount = x.ItemCount,
                CreatedAt = x.CreatedAt,
                Items = x.Items
                    .OrderBy(item => item.CreatedAt)
                    .Select(item => new CustomerOrderRequestItemDTO
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductPriceId = item.ProductPriceId,
                        ProductTitle = item.ProductTitleSnapshot,
                        ProductPriceSize = item.ProductPriceSizeSnapshot,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal,
                        Note = item.Note
                    })
                    .ToList()
            })
            .ToListAsync(ct);
    }

    private async Task<Guid> ResolveCompanyIdAsync(CancellationToken ct)
    {
        if (userContext.IsAdmin)
        {
            return Guid.Empty;
        }

        var companyId = await db.Companies
            .Where(x => x.Owner != null && x.Owner.ApplicationUserId == userContext.UserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);

        return companyId ?? throw new ForbiddenAppException("Şirket bilgisi bulunamadı.");
    }
}
