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

public class ProductMappingProfiles : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<ProductPrice, ProductPriceDTO>();

		config.NewConfig<Product, ProductDTO>();
		
		config.NewConfig<Product, ProductListDTO>()
			.Map(dest => dest.PriceOptionCount, src => src.Prices.Count)
			.Map(dest => dest.CategoryName, src => src.Category != null && src.Category.CategoryLibraryItem != null ? src.Category.CategoryLibraryItem.Title : null)
			.Map(dest => dest.MenuId, src => src.Category != null ? src.Category.MenuId : (Guid?)null)
			.Map(dest => dest.MenuName, src => src.Category != null && src.Category.Menu != null ? src.Category.Menu.Title : null)
			.Map(dest => dest.CompanyId, src => src.Category != null && src.Category.Menu != null ? src.Category.Menu.CompanyId : (Guid?)null)
			.Map(dest => dest.CompanyName, src => src.Category != null && src.Category.Menu != null && src.Category.Menu.Company != null ? src.Category.Menu.Company.Title : null)
			.Map(dest => dest.StoreId, src => src.Category != null && src.Category.Menu != null ? src.Category.Menu.StoreId : (Guid?)null)
			.Map(dest => dest.StoreName, src => src.Category != null && src.Category.Menu != null && src.Category.Menu.Store != null ? src.Category.Menu.Store.Title : null);

		config.NewConfig<CreateProductCommand, Product>()
			.Ignore(dest => dest.Prices);
		config.NewConfig<UpdateProductCommand, Product>()
			.Ignore(dest => dest.Prices);
	}
}
