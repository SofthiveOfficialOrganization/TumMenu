using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class RemoveCategoryFromMenuCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class RemoveCategoryFromMenuValidator : AbstractValidator<RemoveCategoryFromMenuCommand>
{
    public RemoveCategoryFromMenuValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class RemoveCategoryFromMenuHandler(
    IRepository<Category> repoCategory
) : IRequestHandler<RemoveCategoryFromMenuCommand, bool>
{
    public async Task<bool> Handle(RemoveCategoryFromMenuCommand req, CancellationToken ct)
    {
        var category = await repoCategory.GetByIdAsync(req.Id, ct);
        if (category == null)
            throw new NotFoundAppException("Kategori bulunamadı.");

        repoCategory.SoftDelete(category);
        return true;
    }
}
