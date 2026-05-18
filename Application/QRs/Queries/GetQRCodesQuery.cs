using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetQRCodesQuery : IRequest<List<QRListItemDTO>>;

public class QRListItemDTO
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public string PublicKey { get; set; } = null!;
    public string StoreTitle { get; set; } = null!;
    public string CompanyTitle { get; set; } = null!;
    public string? TargetUrl { get; set; }
    public string? BaseDomain { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetQRCodesQueryHandler(IRepository<QRCode> repoQR) : IRequestHandler<GetQRCodesQuery, List<QRListItemDTO>>
{
    public async Task<List<QRListItemDTO>> Handle(GetQRCodesQuery request, CancellationToken ct)
    {
        return await repoQR.Query(tracked: false)
            .Include(q => q.Store)
                .ThenInclude(s => s != null ? s.Company : null)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QRListItemDTO
            {
                Id = q.Id,
                StoreId = q.StoreId,
                PublicKey = q.PublicKey,
                StoreTitle = q.Store != null ? q.Store.Title : "N/A",
                CompanyTitle = (q.Store != null && q.Store.Company != null) ? q.Store.Company.Title : "N/A",
                TargetUrl = q.TargetUrl,
                BaseDomain = q.BaseDomain,
                CreatedAt = q.CreatedAt.DateTime 
            })
            .ToListAsync(ct);
    }
}
