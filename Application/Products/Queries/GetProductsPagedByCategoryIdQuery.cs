using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Products.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Queries;

public sealed record GetProductsPagedByCategoryIdQuery(
	Guid CategoryId
) : PageRequest, IRequest<PaginatedListDTO<ProductDTO>>;

public class GetProductsByCategoryIdHandler(
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<GetProductsPagedByCategoryIdQuery, PaginatedListDTO<ProductDTO>>
{
	public async Task<PaginatedListDTO<ProductDTO>> Handle(GetProductsPagedByCategoryIdQuery req, CancellationToken ct)
	{
		var products = await repoProduct.GetPageListAsync(
			req,
			p => p.CategoryId == req.CategoryId,
			orderBy: p => p.OrderBy(p => p.SortOrder),
			ct: ct
			);
		var productListDTO = mapper.Map<PaginatedListDTO<ProductDTO>>(products);
		return productListDTO;
	}
}
