using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Products.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Queries;

public sealed class GetAllProductsPagedQuery : PageRequest, IRequest<PaginatedListDTO<ProductDTO>>
{
	public string? Search { get; set; }
	public Guid? TagId { get; set; }
}

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
				(string.IsNullOrEmpty(req.Search) ||
				p.Title.Contains(req.Search) ||
				p.Slug.Contains(req.Search)) &&
				(req.TagId == null || p.ProductTags.Any(pt => pt.TagId == req.TagId)),
			orderBy: p => p.OrderBy(p => p.SortOrder),
			include: query => query.Include(p => p.Prices),
			ct: ct
			);
		var productListDTO = mapper.Map<PaginatedListDTO<ProductDTO>>(products);
		return productListDTO;
	}
}
