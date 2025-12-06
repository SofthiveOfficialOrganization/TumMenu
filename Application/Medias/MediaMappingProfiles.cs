using Application.Medias.DTOs;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Medias;

public class MediaMappingProfiles
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Media, MediaDTO>();
	}
}