using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDTO?>;

public class GetCategoryByIdHandler(
    IRepository<Category> repoCategory,
    IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, CategoryDTO?>
{
    public async Task<CategoryDTO?> Handle(GetCategoryByIdQuery req, CancellationToken ct)
    {
        var category = await repoCategory.GetByIdAsync(req.CategoryId, ct);
        if(category is null)
            throw new NotFoundAppException($"Kategori bulunamadı.");

        return mapper.Map<CategoryDTO?>(category);
    }
}
