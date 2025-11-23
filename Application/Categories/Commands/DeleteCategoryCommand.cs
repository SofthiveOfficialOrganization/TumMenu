using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

public class DeleteCategoryCommandHandler(IRepository<Category> repo) : IRequestHandler<DeleteCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = repo.Query().FirstOrDefault(c => c.Id == command.Id);
        if(category == null)
        {
            category.IsDeleted = true;
        }
        return Unit.Value;
    }
}