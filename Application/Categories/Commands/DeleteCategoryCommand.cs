using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;

namespace Application.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Guid>, ITransactionalRequest;

public class DeleteCategoryCommandHandler(
	IRepository<Category> repoCategory
) : IRequestHandler<DeleteCategoryCommand, Guid>
{
	public async Task<Guid> Handle(DeleteCategoryCommand req, CancellationToken ct)
	{
		var category = repoCategory.Query().FirstOrDefault(c => c.Id == req.Id).EnsureFound("Kategori bulunamadı");
		var menuId = category.MenuId;
		repoCategory.SoftDelete(category);
		return menuId;
	}
}