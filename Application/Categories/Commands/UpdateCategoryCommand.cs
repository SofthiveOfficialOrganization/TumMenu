

using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Commands;

public record UpdateCategoryCommand(Guid CategoryId, string Name, string? Description) : IRequest<CategoryDTO>;

public class UpdateCategoryHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<UpdateCategoryCommand, CategoryDTO>
{
	public async Task<CategoryDTO> Handle(UpdateCategoryCommand req, CancellationToken ct)
	{
		var category = repoCategory.Query().FirstOrDefault(c => c.Id == req.CategoryId).EnsureFound("Kategori bulunamadı.");

		mapper.Map(req, category);
		repoCategory.Update(category);
		var categoryDTO = mapper.Map<CategoryDTO>(category);
		return categoryDTO;
	}
}