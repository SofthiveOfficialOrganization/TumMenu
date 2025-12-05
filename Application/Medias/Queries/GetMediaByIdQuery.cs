using Application.Abstractions;
using Application.Common.Helpers;
using Application.Medias.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Medias.Queries;

public sealed record GetMediaByIdQuery(
	Guid Id
) : IRequest<MediaDTO>;

public class GetMediaByIdHandler(
	IRepository<Media> repoMedia,
	IMapper mapper
) : IRequestHandler<GetMediaByIdQuery, MediaDTO>
{
	public async Task<MediaDTO> Handle(GetMediaByIdQuery req, CancellationToken ct)
	{
		var media = await repoMedia.GetByIdAsync(req.Id, ct).EnsureFound("Medya bulunamadı.");
		var mediaDTO = mapper.Map<MediaDTO>(media!);
		return mediaDTO;
	}
}