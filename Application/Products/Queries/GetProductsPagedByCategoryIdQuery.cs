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

public sealed class GetProductsPagedByCategoryIdQuery : PageRequest, IRequest<PaginatedListDTO<ProductDTO>>
{
	public Guid CategoryId { get; set; }
	public string? Search { get; set; }
	public Guid? TagId { get; set; }
}

public class GetProductsByCategoryIdHandler(
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<GetProductsPagedByCategoryIdQuery, PaginatedListDTO<ProductDTO>>
{
	public async Task<PaginatedListDTO<ProductDTO>> Handle(GetProductsPagedByCategoryIdQuery req, CancellationToken ct)
	{
		var products = await repoProduct.GetPageListAsync(
			req,
			p =>
				p.CategoryId == req.CategoryId &&
				(string.IsNullOrEmpty(req.Search) ||
				p.Name.Contains(req.Search) ||
				p.Slug.Contains(req.Search)) &&
				(req.TagId == null || p.ProductTags.Any(pt => pt.TagId == req.TagId)),
			orderBy: p => p.OrderBy(p => p.SortOrder),
			ct: ct
			);
		var productListDTO = mapper.Map<PaginatedListDTO<ProductDTO>>(products);
		return productListDTO;
	}
}
