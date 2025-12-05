using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
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

public sealed record GetMediasPagedByReferanceIdQuery(
	Guid ReferanceId,
	MediaRefType? Type
) : PageRequest, IRequest<PaginatedListDTO<MediaDTO>>;

public class GetMediasPagedByReferanceIdHandler(
	IRepository<Media> repoMedia,
	IMapper mapper
) : IRequestHandler<GetMediasPagedByReferanceIdQuery, PaginatedListDTO<MediaDTO>>
{
	public async Task<PaginatedListDTO<MediaDTO>> Handle(GetMediasPagedByReferanceIdQuery req, CancellationToken ct)
	{
		var medias = await repoMedia.GetPageListAsync(
			req,
			media => media.ReferenceId == req.ReferanceId && (req.Type != null || media.Type == req.Type),
			orderBy: media => media.OrderBy(media => media.SortOrder).ThenByDescending(media => media.CreatedAt),
			ct: ct
		);
		var mediasDTO = mapper.Map<PaginatedListDTO<MediaDTO>>(medias);
		return mediasDTO;
	}
}