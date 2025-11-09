using Application.Common.Base.DTOs;
using Domain.Base;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Base;

public class BaseEntityMapper
{
	public void Register(TypeAdapterConfig config)
	{
		config.ForType<BaseEntity, BaseDTO>();
	}
}
