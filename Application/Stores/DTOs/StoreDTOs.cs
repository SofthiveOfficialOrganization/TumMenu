using Application.Common.Base.DTOs;

namespace Application.Stores.DTOs;

public sealed class StoreLiteDTO : BaseDTO, ISluggableDTO
{
	public string Name { get; set; } = null!;
	public string Slug { get; set; } = null!;
}
