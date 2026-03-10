using Application.Common.Base.DTOs;
using Application.Medias.DTOs;

namespace Application.Companies.DTOs;

public sealed class CompanyDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? OwnerName { get; set; }
	public Guid? OwnerId { get; set; }
	public List<MediaDTO> Medias { get; set; } = [];
}

public sealed class CompanyLiteDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
}

public record CompanyFilterDTO(Guid Id, string Title);
