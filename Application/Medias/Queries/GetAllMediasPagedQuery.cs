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

public sealed class GetAllMediasPagedQuery : PageRequest, IRequest<PaginatedListDTO<MediaDTO>>
{
	public MediaRefType? Type { get; set; }
	public MediaKind? MediaKind { get; set; }
	public string? Extension { get; set; }
	public string? Slot { get; set; }
	public Guid? CompanyId { get; set; }
}

public class GetAllMediasPagedHandler(
	IRepository<Media> repoMedia,
	IMapper mapper
) : IRequestHandler<GetAllMediasPagedQuery, PaginatedListDTO<MediaDTO>>
{
	public async Task<PaginatedListDTO<MediaDTO>> Handle(GetAllMediasPagedQuery req, CancellationToken ct)
	{
		var medias = await repoMedia.GetPageListAsync(
			req,
			media =>
				(req.Type == null || media.Type == req.Type) &&
				(req.MediaKind == null || media.Kind == req.MediaKind) &&
				(string.IsNullOrEmpty(req.Extension) || (media.Extension != null && media.Extension.Contains(req.Extension))) &&
				(string.IsNullOrEmpty(req.Slot) || (media.Slot != null && media.Slot.Contains(req.Slot))) &&
				(req.CompanyId == null || media.CompanyId == req.CompanyId),
			orderBy: media => media.OrderBy(m => m.SortOrder).ThenByDescending(m => m.CreatedAt),
			ct: ct
		);

		var mediasDTO = mapper.Map<PaginatedListDTO<MediaDTO>>(medias);
		return mediasDTO;
	}
}