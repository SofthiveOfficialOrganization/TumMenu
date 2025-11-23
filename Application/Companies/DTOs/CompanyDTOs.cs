using Application.Common.Base.DTOs;

namespace Application.Companies.DTOs;

public sealed record CompanyDTO(
	string Name,
	string Slug,
	Guid? OwnerId
) : BaseDTO, ISluggableDTO;

public sealed record CompanyLiteDTO(
	string Name,
	string Slug
) : BaseDTO, ISluggableDTO;
