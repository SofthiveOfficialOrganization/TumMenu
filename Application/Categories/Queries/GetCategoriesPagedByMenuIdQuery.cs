using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public record GetCategoriesPagedByMenuIdQuery(Guid MenuId) : PageRequest, IRequest<List<CategoryDTO>>;

public class GetCategoriesByMenuIdHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoriesPagedByMenuIdQuery, List<CategoryDTO>>
{
	public async Task<List<CategoryDTO>> Handle(GetCategoriesPagedByMenuIdQuery req, CancellationToken ct)
	{
		var categoryList = await repoCategory.Query()
			.Where(c => c.MenuId == req.MenuId)
			.OrderBy(c => c.SortOrder)
			.ToListAsync(ct);
		//var categoryList = await repoCategory.GetPageListAsync(
		//	req,
		//	c=>c.MenuId == req.MenuId,
		//	c=>c.OrderBy(c=>c.SortOrder),
		//	)
		if(categoryList.Count == 0)
			throw new NotFoundAppException("Bu menuye ait kategori bulunamadı");
		var categoryListDTO = mapper.Map<List<CategoryDTO>>(categoryList);
		return categoryListDTO;
	}
}
