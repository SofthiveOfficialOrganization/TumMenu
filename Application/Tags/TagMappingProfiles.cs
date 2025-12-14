using Application.Tags.Commands;
using Application.Tags.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags;

public class TagMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Tag, TagDTO>();
		config.NewConfig<CreateTagCommand, Tag>();
		config.NewConfig<UpdateTagCommand, Tag>();
	}
}