using Application.Abstractions;
using Application.Common.Helpers;
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

public sealed record GetProductByIdQuery(
	Guid ProductId
) : IRequest<ProductDTO>;

public class GetProductByIdHandler(
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<GetProductByIdQuery, ProductDTO>
{
	public async Task<ProductDTO> Handle(GetProductByIdQuery req, CancellationToken ct)
	{
		var product = (await repoProduct.Query()
			.Include(p => p.Prices)
			.Include(p => p.Medias)
			.FirstOrDefaultAsync(p => p.Id == req.ProductId, ct)).EnsureFound("Ürün bulunamadı.");
		var productDTO = mapper.Map<ProductDTO>(product);
		return productDTO;
	}
}
