using Application.Common.Base.DTOs;
using Domain.Entities;

namespace Application.Menus.DTOs;

public sealed class MenuDTO() : BaseDTO
{
	public string? Name { get; set; }
	public Guid? StoreId { get; set; }
	public Guid? CompanyId { get; set; }
	public Guid? MenuTemplateId { get; set; }
	public ICollection<Media> Medias { get; set; } = [];
	public ICollection<Category> Categories { get; set; } = [];
}