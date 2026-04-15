using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;

namespace Application.Categories.Commands;

public class DeleteCategoryLibraryItemCommand : IRequest<Unit>, ITransactionalRequest
{
	public Guid Id { get; set; }
}

public class DeleteCategoryLibraryItemCommandHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibItem
) : IRequestHandler<DeleteCategoryLibraryItemCommand, Unit>
{
	public Task<Unit> Handle(DeleteCategoryLibraryItemCommand req, CancellationToken ct)
	{
		var category = repoCategoryLibItem.Query(tracked: true).FirstOrDefault(c => c.Id == req.Id).EnsureFound("Kategori bulunamadı");
		repoCategoryLibItem.SoftDelete(category);
		return Task.FromResult(Unit.Value);
	}
}