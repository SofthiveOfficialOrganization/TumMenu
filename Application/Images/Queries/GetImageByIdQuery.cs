using Application.Abstractions;
using Application.Common.Helpers;
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

public sealed record GetImageByIdQuery(
	Guid Id
) : IRequest<ImageDTO>;

public class GetImageByIdHandler(
	IRepository<Image> repoImage,
	IMapper mapper
) : IRequestHandler<GetImageByIdQuery, ImageDTO>
{
	public async Task<ImageDTO> Handle(GetImageByIdQuery req, CancellationToken ct)
	{
		var image = await repoImage.GetByIdAsync(req.Id, ct).EnsureFound("Medya bulunamadı.");
		var imageDTO = mapper.Map<ImageDTO>(image!);
		return imageDTO;
	}
}