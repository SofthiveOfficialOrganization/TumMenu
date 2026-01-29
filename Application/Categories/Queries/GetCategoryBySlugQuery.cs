using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public class GetCategoryBySlugQuery : IRequest<CategoryDTO?>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetCategoryBySlugHandler(
    IRepository<Category> repoCategory,
    IMapper mapper
) : IRequestHandler<GetCategoryBySlugQuery, CategoryDTO?>
{
    public async Task<CategoryDTO?> Handle(GetCategoryBySlugQuery req, CancellationToken ct)
    {
        var category = (await repoCategory.Query()
            .Where(c => c.Slug == req.Slug)
            .FirstOrDefaultAsync(ct)).EnsureFound("Kategori bulunamadı.");

        return mapper.Map<CategoryDTO?>(category);
    }
}
