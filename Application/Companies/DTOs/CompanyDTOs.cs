using Application.Common.Base.DTOs;

namespace Application.Companies.DTOs;

public sealed record CompanyLiteDTO(
    string Name,
    string Slug
) : BaseDTO, ISluggableDTO;
