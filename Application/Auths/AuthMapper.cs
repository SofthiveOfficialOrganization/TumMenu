using Application.Auths.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auths
{
	public class AuthMapper
	{
		public AuthMapper(TypeAdapterConfig config)
		{
			config.NewConfig<ApplicationUser, BasicApplicationUserDTO>();
		}
	}
}
