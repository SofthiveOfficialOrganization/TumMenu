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

public class MenuMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Menu, MenuDTO>();
		config.NewConfig<CreateMenuToCompanyCommand, Menu>();
		config.NewConfig<CreateMenuToStoreCommand, Menu>();
		config.NewConfig<CopyMenuCommand, Menu>();
	}
}