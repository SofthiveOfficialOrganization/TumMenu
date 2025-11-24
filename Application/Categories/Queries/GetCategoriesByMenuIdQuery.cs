using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public record GetCategoriesByMenuIdQuery(Guid MenuId) : IRequest<List<CategoryDTO>>;

public class GetCategoriesByMenuIdHandler(
    IRepository<Category> repoCategory,
    IMapper mapper
) : IRequestHandler<GetCategoriesByMenuIdQuery, List<CategoryDTO>>
{
    public async Task<List<CategoryDTO>> Handle(GetCategoriesByMenuIdQuery req, CancellationToken ct)
    {
        var categoryList = await repoCategory.Query()
            .Where(c => c.MenuId == req.MenuId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
        if(categoryList.Count == 0)
            throw new NotFoundAppException("Bu menuye ait kategori bulunamadı");
        var categoryListDTO = mapper.Map<List<CategoryDTO>>(categoryList);
        return categoryListDTO;
    }
}
