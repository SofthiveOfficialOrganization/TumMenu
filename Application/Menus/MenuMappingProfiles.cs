using Application.Menus.Commands;
using Application.Menus.DTOs;
using Application.Owners.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Menus;

public class MenuMappingProfiles : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Menu, MenuDTO>()
			.Map(dest => dest.StoreName, src => src.Store != null ? src.Store.Title : null)
			.Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Title : null)
			.Map(dest => dest.StoreSlug, src => src.Store != null ? src.Store.Slug : null)
			.Map(dest => dest.CompanySlug, src => (src.Store != null && src.Store.Company != null) ? src.Store.Company.Slug : (src.Company != null ? src.Company.Slug : null));
		config.NewConfig<CreateMenuToCompanyCommand, Menu>();
		config.NewConfig<CreateMenuToStoreCommand, Menu>();
		config.NewConfig<CopyMenuCommand, Menu>();
	}
}