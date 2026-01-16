

using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public class UpdateCategoryCommand : IRequest<CategoryDTO>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? Slug { get; set; }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Slug)
            .MaximumLength(100).WithMessage("Şirket slug'ı en fazla 100 karakter olabilir.")
            .Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
                .When(c => !string.IsNullOrWhiteSpace(c.Slug));
    }
}
public class UpdateCategoryHandler(
    IRepository<Category> repoCategory,
    IMapper mapper
) : IRequestHandler<UpdateCategoryCommand, CategoryDTO>
{
    public async Task<CategoryDTO> Handle(UpdateCategoryCommand req, CancellationToken ct)
    {
        var category = await repoCategory.Query()
            .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        category = category.EnsureFound("Kategori bulunamadı.");

        mapper.Map(req, category);
        var effectiveSlug = string.IsNullOrWhiteSpace(req.Slug)
            ? SlugHelper.Slugify(req.Name)
            : req.Slug;

        category.Slug = effectiveSlug;
        var exists = await repoCategory.Query().AnyAsync(c => c.MenuId == category.MenuId && c.Slug == category.Slug && c.Id != req.Id, ct);
        if (exists)
            throw new AlreadyExistsAppException(
                $"Bu menüde '{category.Slug}' slug'ına sahip bir kategori bulunmakta. Farklı bir slug değeri girin."
            );
        repoCategory.Update(category);
        var categoryDTO = mapper.Map<CategoryDTO>(category);
        return categoryDTO;
    }
}