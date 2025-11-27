using Application.Common.Base.DTOs;
namespace Application.Categories.DTOs;

public sealed class CategoryDTO : BaseDTO
{
	public Guid MenuId { get; set; }
	public string Name { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public int SortOrder { get; set; }
}
