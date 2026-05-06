using Application.Common.Base.DTOs;

namespace Application.Products.DTOs;

public sealed class ProductListDTO : BaseDTO
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public int PriceOptionCount { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Allergens { get; set; }
    public bool? IsVegan { get; set; }
    public bool? IsVegetarian { get; set; }

    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public Guid? MenuId { get; set; }
    public string? MenuName { get; set; }

    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }

    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
}
