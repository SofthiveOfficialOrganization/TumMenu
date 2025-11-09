using Application.Companies.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Companies
{
	public class CompanyMapping
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Company, CompanyLiteDTO>();
		}
	}
}
