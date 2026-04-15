using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetMyQRInfoQuery : IRequest<List<MyQRDTO>>;

public class MyQRDTO
{
    public Guid StoreId { get; set; }
    public string StoreTitle { get; set; } = null!;
    public string CompanyTitle { get; set; } = null!;
    public string QRKey { get; set; } = null!;
    public string PublicUrl { get; set; } = null!;
    public string? BaseDomain { get; set; }
    public string MenuUrl { get; set; } = null!;
}

public class GetMyQRInfoQueryHandler(
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetMyQRInfoQuery, List<MyQRDTO>>
{
    public async Task<List<MyQRDTO>> Handle(GetMyQRInfoQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId)) return new List<MyQRDTO>();

        var stores = await repoStore.Query(tracked: false)
            .Include(s => s.Company)
                .ThenInclude(c => c.Owner)
            .Include(s => s.QRCode)
            .Where(s => s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        return stores
            .Where(s => s.QRCode != null && s.Company != null)
            .Select(store => new MyQRDTO
            {
                StoreId = store.Id,
                StoreTitle = store.Title,
                CompanyTitle = store.Company!.Title,
                QRKey = store.QRCode!.PublicKey,
                PublicUrl = $"/q/{store.QRCode!.PublicKey}",
                BaseDomain = store.QRCode!.BaseDomain,
                MenuUrl = $"/{store.Company!.Slug}/{store.Slug}"
            })
            .ToList();
    }
}
