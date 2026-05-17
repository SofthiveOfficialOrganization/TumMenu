using Application.Staffs.DTOs;
using Application.Stores.Commands;
using Application.Stores.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Stores;

public class StoreMappingProfiles : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<CreateStoreCommand, Store>()
			.Ignore(dest => dest.SocialLinks);
		config.NewConfig<UpdateStoreCommand, Store>()
			.Ignore(dest => dest.SocialLinks);
		config.NewConfig<Store, StoreDTO>();
		config.NewConfig<Store, StoreLiteDTO>();
	}
}
