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

public sealed record GetAllProductsPagedQuery(
	string? Search
) : PageRequest, IRequest<PaginatedListDTO<ProductDTO>>;

public class GetAllProductsPagedHandler(
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<GetAllProductsPagedQuery, PaginatedListDTO<ProductDTO>>
{
	public async Task<PaginatedListDTO<ProductDTO>> Handle(GetAllProductsPagedQuery req, CancellationToken ct)
	{
		var products = await repoProduct.GetPageListAsync(
			req,
			p =>
				string.IsNullOrEmpty(req.Search) ||
				p.Name.Contains(req.Search) ||
				p.Slug.Contains(req.Search),
			orderBy: p => p.OrderBy(p => p.SortOrder),
			ct: ct
			);
		var productListDTO = mapper.Map<PaginatedListDTO<ProductDTO>>(products);
		return productListDTO;
	}
}