using Application.Common.Base.DTOs;

namespace Application.Categories.DTOs;

public sealed class CategoryListDTO : BaseDTO
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconKey { get; set; }

    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = null!;
    
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;
    
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    
    public int ProductCount { get; set; }
}
