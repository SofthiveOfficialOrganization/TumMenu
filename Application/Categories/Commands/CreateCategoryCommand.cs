using Application.Abstractions;
using Application.Categories.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public record CreateCategoryCommand(Guid MenuId, string Name, int SortOrder) : IRequest<CategoryDTO>;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateCategoryHandler(IRepository<Category> repo, IUnitOfWork uow, IMapper mapper) : IRequestHandler<CreateCategoryCommand, CategoryDTO>
{
    public async Task<CategoryDTO> Handle(CreateCategoryCommand req, CancellationToken ct)
    {
        var exists = await repo.Query().AnyAsync(c => c.MenuId == req.MenuId && c.Slug == SlugHelper.Slugify(req.Name), ct);
        if(exists)
            throw new InvalidOperationException("Bu menüde aynı isimde bir kategori zaten var.");

        var entity = mapper.Map<Category>(req);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<CategoryDTO>(entity);
    }
}
