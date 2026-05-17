using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands;

public enum BulkPriceUpdateType
{
    FixedAmount = 1,
    Percentage = 2
}

public enum BulkPriceRoundingStrategy
{
    None = 0,
    Nearest5 = 5,
    Nearest10 = 10
}

public sealed class BulkUpdateMenuProductPricesCommand : IRequest<BulkUpdateMenuProductPricesResult>, ITransactionalRequest, IEntityAuditableCommand
{
    public string ActionName => "Toplu fiyat güncellendi";
    public Guid EntityId => MenuId;

    public Guid MenuId { get; set; }
    public BulkPriceUpdateType UpdateType { get; set; } = BulkPriceUpdateType.Percentage;
    public decimal Amount { get; set; }
    public BulkPriceRoundingStrategy RoundingStrategy { get; set; } = BulkPriceRoundingStrategy.None;
}

public sealed record BulkUpdateMenuProductPricesResult(int ProductCount, int PriceCount);

public sealed class BulkUpdateMenuProductPricesCommandValidator : AbstractValidator<BulkUpdateMenuProductPricesCommand>
{
    public BulkUpdateMenuProductPricesCommandValidator()
    {
        RuleFor(x => x.MenuId).NotEmpty();

        RuleFor(x => x.UpdateType)
            .IsInEnum();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Artış değeri 0'dan büyük olmalı.")
            .LessThanOrEqualTo(1000)
            .WithMessage("Artış değeri en fazla 1000 olabilir.");

        RuleFor(x => x.RoundingStrategy)
            .IsInEnum();

        RuleFor(x => x.RoundingStrategy)
            .Equal(BulkPriceRoundingStrategy.None)
            .When(x => x.UpdateType == BulkPriceUpdateType.FixedAmount)
            .WithMessage("Yuvarlama sadece yüzdelik artışta kullanılabilir.");
    }
}

public sealed class BulkUpdateMenuProductPricesCommandHandler(
    IRepository<Menu> repoMenu,
    IRepository<Product> repoProduct,
    IUserContext userContext
) : IRequestHandler<BulkUpdateMenuProductPricesCommand, BulkUpdateMenuProductPricesResult>
{
    private const decimal MaximumPrice = 9999m;

    public async Task<BulkUpdateMenuProductPricesResult> Handle(BulkUpdateMenuProductPricesCommand req, CancellationToken ct)
    {
        var menu = await repoMenu.Query()
            .Include(m => m.Company).ThenInclude(c => c!.Owner)
            .Include(m => m.Store).ThenInclude(s => s!.Company).ThenInclude(c => c.Owner)
            .FirstOrDefaultAsync(m => m.Id == req.MenuId, ct)
            ?? throw new NotFoundAppException("Menü bulunamadı.");

        if (!userContext.IsAdmin && !UserOwnsMenu(menu))
            throw new ForbiddenAppException("Bu menünün fiyatlarını güncelleme yetkiniz yok.");

        var products = await repoProduct.Query(tracked: true)
            .Include(p => p.Prices)
            .Where(p => p.Category.MenuId == req.MenuId)
            .ToListAsync(ct);

        if (products.Count == 0)
            throw new UnprocessableAppException("Bu menüde güncellenecek ürün bulunamadı.");

        var calculatedPrices = products
            .SelectMany(product => new[] { CalculateNewPrice(product.BasePrice, req) }
                .Concat(product.Prices.Select(price => CalculateNewPrice(price.Price, req))))
            .ToList();

        if (calculatedPrices.Any(price => price > MaximumPrice))
            throw new UnprocessableAppException("Bu işlem sonucunda bazı fiyatlar 9999 TL sınırını geçiyor. Daha düşük bir artış değeri girin.");

        var priceCount = 0;
        foreach (var product in products)
        {
            product.BasePrice = CalculateNewPrice(product.BasePrice, req);
            priceCount++;

            foreach (var priceOption in product.Prices)
            {
                priceOption.Price = CalculateNewPrice(priceOption.Price, req);
                priceCount++;
            }
        }

        return new BulkUpdateMenuProductPricesResult(products.Count, priceCount);
    }

    private bool UserOwnsMenu(Menu menu)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (menu.Company?.Owner?.ApplicationUserId == userId)
            return true;

        return menu.Store?.Company?.Owner?.ApplicationUserId == userId;
    }

    private static decimal CalculateNewPrice(decimal currentPrice, BulkUpdateMenuProductPricesCommand req)
    {
        var updatedPrice = req.UpdateType == BulkPriceUpdateType.FixedAmount
            ? currentPrice + req.Amount
            : currentPrice + (currentPrice * req.Amount / 100m);

        if (req.UpdateType == BulkPriceUpdateType.Percentage && req.RoundingStrategy != BulkPriceRoundingStrategy.None)
            updatedPrice = RoundToNearest(updatedPrice, (int)req.RoundingStrategy);

        return Math.Round(updatedPrice, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundToNearest(decimal value, int multiple)
    {
        if (multiple <= 0)
            return value;

        return Math.Round(value / multiple, 0, MidpointRounding.AwayFromZero) * multiple;
    }
}
