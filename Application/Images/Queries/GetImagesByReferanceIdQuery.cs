using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Images.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Images.Queries;

public sealed record GetImagesByReferanceIdQuery(
	Guid ReferanceId,
	ImageRefType? Type
) : PageRequest, IRequest<PaginatedListDTO<ImageDTO>>;

public class GetImagesByReferanceIdHandler(
	IRepository<Image> repoImage,
	IMapper mapper
) : IRequestHandler<GetImagesByReferanceIdQuery, PaginatedListDTO<ImageDTO>>
{
	public async Task<PaginatedListDTO<ImageDTO>> Handle(GetImagesByReferanceIdQuery req, CancellationToken ct)
	{
		var images = await repoImage.GetPageListAsync(
			req,
			image => image.ReferenceId == req.ReferanceId && (req.Type != null && image.Type == req.Type),
			orderBy: image => image.OrderBy(image => image.SortOrder),
			ct: ct
		);
		var imagesDTO = mapper.Map<PaginatedListDTO<ImageDTO>>(images);
		return imagesDTO;
	}
}