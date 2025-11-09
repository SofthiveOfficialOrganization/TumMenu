using Application.Common.Base.DTOs;
namespace Application.Categories.DTOs;

public sealed record CategoryDTO(Guid MenuId, string Name, string Slug, int SortOrder) : BaseDTO;
