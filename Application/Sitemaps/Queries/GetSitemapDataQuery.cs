using MediatR;

namespace Application.Sitemaps.Queries;

public record GetSitemapDataQuery : IRequest<SitemapDataDTO>;

public class SitemapDataDTO
{
    public List<SitemapItemDTO> Items { get; set; } = [];
}

public class SitemapItemDTO
{
    public string CompanySlug { get; set; } = null!;
    public string StoreSlug { get; set; } = null!;
    public string? CategorySlug { get; set; }
    public string? ProductSlug { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public SitemapItemType Type { get; set; }
}

public enum SitemapItemType
{
    Store,
    Category,
    Product
}
