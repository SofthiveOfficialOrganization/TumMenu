using Application.Common.Base.DTOs;

namespace Application.Companies.DTOs;

public sealed class CompanyDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? OwnerName { get; set; }
	public Guid? OwnerId { get; set; }
}

public sealed class CompanyLiteDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
}
