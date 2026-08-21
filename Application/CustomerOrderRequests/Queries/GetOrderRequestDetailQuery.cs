using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.CustomerOrderRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Queries;

public sealed record GetOrderRequestDetailQuery(Guid Id)
    : IRequest<CustomerOrderRequestDetailDTO>, IAuthorizedRequest;

public sealed class GetOrderRequestDetailQueryHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : IRequestHandler<GetOrderRequestDetailQuery, CustomerOrderRequestDetailDTO>
{
    public async Task<CustomerOrderRequestDetailDTO> Handle(GetOrderRequestDetailQuery request, CancellationToken ct)
    {
        var order = await db.CustomerOrderRequests
            .Include(x => x.Store)
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (order is null)
        {
            throw new NotFoundAppException("Sipariş talebi bulunamadı.");
        }

        if (!userContext.IsAdmin)
        {
            var companyId = await db.Companies
                .Where(x => x.Owner != null && x.Owner.ApplicationUserId == userContext.UserId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (companyId != order.CompanyId)
            {
                throw new ForbiddenAppException("Bu sipariş talebini görüntüleyemezsiniz.");
            }
        }

        return new CustomerOrderRequestDetailDTO
        {
            Id = order.Id,
            CompanyId = order.CompanyId,
            StoreId = order.StoreId,
            StoreName = order.Store.Title,
            CustomerName = order.CustomerName,
            TableNumber = order.TableNumber,
            Note = order.Note,
            Status = order.Status,
            Subtotal = order.Subtotal,
            ItemCount = order.ItemCount,
            CreatedAt = order.CreatedAt,
            Items = order.Items
                .OrderBy(x => x.CreatedAt)
                .Select(x => new CustomerOrderRequestItemDTO
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductPriceId = x.ProductPriceId,
                    ProductTitle = x.ProductTitleSnapshot,
                    ProductPriceSize = x.ProductPriceSizeSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    LineTotal = x.LineTotal,
                    Note = x.Note
                })
                .ToList()
        };
    }
}
