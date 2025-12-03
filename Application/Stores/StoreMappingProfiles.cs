using Application.Staffs.DTOs;
using Application.Stores.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Stores;

public class StoreMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Store, StoreDTO>();
		config.NewConfig<Store, StoreLiteDTO>();
	}
}
