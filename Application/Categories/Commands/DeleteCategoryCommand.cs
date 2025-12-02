using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;

namespace Application.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>, ITransactionalRequest;

public class DeleteCategoryCommandHandler(
	IRepository<Category> repoCategory
) : IRequestHandler<DeleteCategoryCommand, Unit>
{
	public async Task<Unit> Handle(DeleteCategoryCommand req, CancellationToken ct)
	{
		var category = repoCategory.Query().FirstOrDefault(c => c.Id == req.Id).EnsureFound("Kategori bulunamadı");

		repoCategory.SoftDelete(category);
		return Unit.Value;
	}
}