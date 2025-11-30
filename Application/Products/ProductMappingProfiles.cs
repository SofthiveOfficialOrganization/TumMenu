using Application.Products.Commands;
using Application.Products.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products;


public class ProductMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Product, ProductDTO>();
		config.NewConfig<CreateProductCommand, Product>();
		config.NewConfig<UpdateProductCommand, Product>();
	}
}

