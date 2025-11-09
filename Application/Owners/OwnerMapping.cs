using Application.Owners.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Owners;
public class OwnerMapping
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Owner, OwnerDTO>();
	}
}