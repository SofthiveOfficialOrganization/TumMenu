using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public class CreateCategoryCommand : IRequest<CategoryDTO>, ITransactionalRequest
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? Slug { get; set; }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Slug)
            .MaximumLength(100).WithMessage("Şirket slug'ı en fazla 100 karakter olabilir.")
            .Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
                .When(c => !string.IsNullOrWhiteSpace(c.Slug));
    }
}

public class CreateCategoryHandler(
    IRepository<Category> repoCategory,
    IMapper mapper,
    IRepository<Menu> repoMenu
) : IRequestHandler<CreateCategoryCommand, CategoryDTO>
{
    public async Task<CategoryDTO> Handle(CreateCategoryCommand req, CancellationToken ct)
    {
        var menuExists = await repoMenu.ExistsAsync(m => m.Id == req.MenuId, ct);
        if (!menuExists)
            throw new UnprocessableAppException("Kategorinin ekleneceği menü bulunamadı.");

        var effectiveSlug = string.IsNullOrWhiteSpace(req.Slug)
            ? SlugHelper.Slugify(req.Name)
            : req.Slug;

        var exists = await repoCategory.Query()
            .AnyAsync(c => c.MenuId == req.MenuId && c.Slug == effectiveSlug, ct);

        if (exists)
            throw new AlreadyExistsAppException(
                $"Bu menüde '{effectiveSlug}' slug'ına sahip bir kategori bulunmakta. Farklı bir slug değeri girin."
            );

        var entity = mapper.Map<Category>(req);
        entity.Slug = effectiveSlug;

        await repoCategory.AddAsync(entity, ct);
        return mapper.Map<CategoryDTO>(entity);
    }
}
