using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.CustomerOrderRequests.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Commands;

public sealed class CreateCustomerOrderRequestCommand : IRequest<CustomerOrderRequestDetailDTO>
{
    public string? SessionToken { get; set; }
    public Guid StoreId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string TableNumber { get; set; } = null!;
    public string? Note { get; set; }
    public List<CreateCustomerOrderRequestItemDTO> Items { get; set; } = [];
}

public sealed class CreateCustomerOrderRequestCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCustomerOrderRequestCommand, CustomerOrderRequestDetailDTO>
{
    public async Task<CustomerOrderRequestDetailDTO> Handle(CreateCustomerOrderRequestCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SessionToken))
        {
            throw new UnauthorizedAppException("Sipariş göndermek için QR kodu tekrar okutun.");
        }

        var customerName = NormalizeRequired(request.CustomerName, 150, "Ad alanı zorunludur.");
        var tableNumber = NormalizeRequired(request.TableNumber, 40, "Masa no zorunludur.");
        var note = NormalizeOptional(request.Note, 1000);

        if (request.Items.Count == 0)
        {
            throw new UnprocessableAppException("Sepetiniz boş.");
        }

        if (request.Items.Count > 50)
        {
            throw new UnprocessableAppException("Tek seferde en fazla 50 satır gönderilebilir.");
        }

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
        var tokenHash = OrderSessionToken.Hash(request.SessionToken);
        var session = await db.QrOrderSessions
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                !x.IsRevoked &&
                x.ExpiresAt > now,
                ct);

        if (session is null)
        {
            throw new UnauthorizedAppException("QR sipariş süreniz doldu. Lütfen QR kodu tekrar okutun.");
        }

        if (session.StoreId != request.StoreId)
        {
            throw new ForbiddenAppException("Bu QR oturumu farklı bir dükkan için geçerli.");
        }

        var store = await db.Stores
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == request.StoreId, ct);

        if (store is null)
        {
            throw new NotFoundAppException("Dükkan bulunamadı.");
        }

        var menuId = await ResolveMenuIdAsync(store, ct);
        if (menuId is null)
        {
            throw new NotFoundAppException("Bu dükkan için aktif bir menü bulunamadı.");
        }

        var order = new CustomerOrderRequest
        {
            Id = Guid.NewGuid(),
            CompanyId = store.CompanyId,
            StoreId = store.Id,
            QrOrderSessionId = session.Id,
            CustomerName = customerName,
            TableNumber = tableNumber,
            Note = note,
            Status = CustomerOrderRequestStatus.New
        };
        order.Created();

        foreach (var requestedItem in request.Items)
        {
            if (requestedItem.Quantity is < 1 or > 99)
            {
                throw new UnprocessableAppException("Ürün adedi 1 ile 99 arasında olmalıdır.");
            }

            var product = await db.Products
                .Include(x => x.Category)
                    .ThenInclude(x => x.Menu)
                .Include(x => x.Prices)
                .FirstOrDefaultAsync(x =>
                    x.Id == requestedItem.ProductId &&
                    x.IsActive &&
                    x.Category.IsActive &&
                    x.Category.MenuId == menuId.Value,
                    ct);

            if (product is null)
            {
                throw new UnprocessableAppException("Sepette artık sipariş verilemeyen bir ürün var.");
            }

            ProductPrice? selectedPrice = null;
            if (requestedItem.ProductPriceId.HasValue)
            {
                selectedPrice = product.Prices.FirstOrDefault(x => x.Id == requestedItem.ProductPriceId.Value);
                if (selectedPrice is null)
                {
                    throw new UnprocessableAppException("Ürün fiyat seçeneği geçerli değil.");
                }
            }

            var unitPrice = selectedPrice?.Price ?? product.BasePrice;
            var quantity = requestedItem.Quantity;
            var lineTotal = unitPrice * quantity;
            var item = new CustomerOrderRequestItem
            {
                Id = Guid.NewGuid(),
                OrderRequestId = order.Id,
                ProductId = product.Id,
                ProductPriceId = selectedPrice?.Id,
                ProductTitleSnapshot = product.Title,
                ProductPriceSizeSnapshot = selectedPrice?.Size,
                UnitPrice = unitPrice,
                Quantity = quantity,
                LineTotal = lineTotal,
                Note = NormalizeOptional(requestedItem.Note, 500)
            };
            item.Created();
            order.Items.Add(item);
            order.Subtotal += lineTotal;
            order.ItemCount += quantity;
        }

        session.LastUsedAt = now;
        db.QrOrderSessions.Update(session);
        await db.CustomerOrderRequests.AddAsync(order, ct);
        await db.SaveChangesAsync(ct);

        return ToDetailDto(order, store.Title);
    }

    private async Task<Guid?> ResolveMenuIdAsync(Store store, CancellationToken ct)
    {
        var activeStoreMenuId = await db.Menus
            .Where(m => m.StoreId == store.Id && m.Status == MenuStatus.Active)
            .OrderBy(m => m.CreatedAt)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync(ct);

        if (activeStoreMenuId.HasValue)
        {
            return activeStoreMenuId.Value;
        }

        if (store.Company.DefaultMainMenuId.HasValue)
        {
            var defaultMainMenuId = await db.Menus
                .Where(m =>
                    m.Id == store.Company.DefaultMainMenuId.Value &&
                    m.CompanyId == store.Company.Id &&
                    m.Status == MenuStatus.MainMenu)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(ct);

            if (defaultMainMenuId.HasValue)
            {
                return defaultMainMenuId.Value;
            }
        }

        return await db.Menus
            .Where(m => m.CompanyId == store.Company.Id && m.Status == MenuStatus.MainMenu)
            .OrderBy(m => m.CreatedAt)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static CustomerOrderRequestDetailDTO ToDetailDto(CustomerOrderRequest order, string storeName)
    {
        return new CustomerOrderRequestDetailDTO
        {
            Id = order.Id,
            CompanyId = order.CompanyId,
            StoreId = order.StoreId,
            StoreName = storeName,
            CustomerName = order.CustomerName,
            TableNumber = order.TableNumber,
            Note = order.Note,
            Status = order.Status,
            Subtotal = order.Subtotal,
            ItemCount = order.ItemCount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(x => new CustomerOrderRequestItemDTO
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
            }).ToList()
        };
    }

    private static string NormalizeRequired(string? value, int maxLength, string message)
    {
        value = value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UnprocessableAppException(message);
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        value = value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
