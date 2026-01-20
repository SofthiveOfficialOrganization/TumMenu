using Application.Common.Base.DTOs;
namespace Application.Categories.DTOs;

public sealed class CategoryDTO : BaseDTO
{
	public Guid MenuId { get; set; }
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? Description { get; set; }
	public int SortOrder { get; set; }
}
