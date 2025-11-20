using Application.Companies.DTOs;
using Application.Staffs.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs
{
	public class StaffMapping
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Staff, StaffDTO>();
		}
	}
}
