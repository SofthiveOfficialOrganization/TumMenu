using Application.Common.Base.DTOs;

namespace Application.Companies.DTOs;

public sealed class CompanyDTO : BaseDTO, ISluggableDTO
{
	public string Name { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public Guid? OwnerId { get; set; }
}

public sealed class CompanyLiteDTO : BaseDTO, ISluggableDTO
{
	public string Name { get; set; } = null!;
	public string Slug { get; set; } = null!;
}
