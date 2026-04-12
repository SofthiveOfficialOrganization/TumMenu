namespace Application.Stores.DTOs;

public sealed class StoreSearchResultDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string CompanySlug { get; set; } = null!;
	public string? CompanyName { get; set; }
	public string? ImageUrl { get; set; }
	public string? City { get; set; }
	public string? District { get; set; }
	public double? DistanceKm { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public MatchedProductDTO? MatchedProduct { get; set; }
}

public sealed class MatchedProductDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public decimal BasePrice { get; set; }
	public string? ImageUrl { get; set; }
}

public sealed class StoreSearchResultListDTO
{
	public List<StoreSearchResultDTO> Items { get; set; } = [];
	public int TotalCount { get; set; }
	public int Page { get; set; }
	public int PageSize { get; set; }
	public bool HasNext { get; set; }
}
