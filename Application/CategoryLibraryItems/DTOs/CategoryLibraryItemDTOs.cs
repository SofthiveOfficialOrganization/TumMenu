using Application.Common.Base.DTOs;
using Application.Products.DTOs;

namespace Application.Categories.DTOs;

public sealed class CategoryLibraryItemDTO : BaseDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? Description { get; set; }
	public string? IconKey { get; set; }
	public Guid? ParentId { get; set; }
}

public sealed class CategoryDTO : BaseDTO
{
	public Guid MenuId { get; set; }
	public Guid CategoryLibraryItemId { get; set; }
	public CategoryLibraryItemDTO CategoryLibraryItem { get; set; } = null!;
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public ICollection<ProductDTO> Products { get; set; } = [];
}
