using Application.Common.Base.DTOs;
using Application.Medias.DTOs;
using Application.Categories.DTOs;
using Application.MenuDesigns.DTOs;
using Domain.Entities;

namespace Application.Menus.DTOs;

public sealed class MenuDTO : BaseDTO
{
	public string? Title { get; set; }
	public Guid? StoreId { get; set; }
	public Guid? CompanyId { get; set; }
	public Guid? MenuDesignId { get; set; }
	public MenuDesignDTO? MenuDesign { get; set; }
	public List<MediaDTO> Medias { get; set; } = [];
	public ICollection<CategoryDTO> Categories { get; set; } = [];
	public MenuStatus Status { get; set; }
	public string? StoreName { get; set; }
	public string? CompanyName { get; set; }
	public string? StoreSlug { get; set; }
	public string? CompanySlug { get; set; }
	public bool IsDefaultCompanyMenu { get; set; }
}
