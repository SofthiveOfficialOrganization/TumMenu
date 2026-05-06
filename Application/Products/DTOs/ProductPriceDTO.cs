using Application.Common.Base.DTOs;

namespace Application.Products.DTOs;

public sealed class ProductPriceDTO : BaseDTO
{
	public string? Size { get; set; }
	public decimal Price { get; set; }
}

public sealed class ProductPriceInputDTO
{
	public string? Size { get; set; }
	public decimal? Price { get; set; }
}
