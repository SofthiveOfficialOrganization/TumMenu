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

namespace Application.Medias.Commands;

public sealed record UpdateMediaCommand(
	Guid Id,
	string MediaUrl,
	string? AltText,
	int SortOrder,
	string? Slot,
	int? Width,
	int? Height,
	long? FileSize,
	string? Extension,
	string? MimeType,
	Guid ReferenceId,
	MediaRefType MediaRefType
) : IRequest<MediaDTO>, ITransactionalRequest;

public class UpdateMediaCommandHandler(
	IRepository<Media> mediaRepository,
	IMapper mapper
	) : IRequestHandler<UpdateMediaCommand, MediaDTO>
{
	public async Task<MediaDTO> Handle(UpdateMediaCommand req, CancellationToken ct)
	{
		var media = await mediaRepository.GetByIdAsync(req.Id, ct).EnsureFound("Medya içeriği bulunamadı");
		mapper.Map(req, media);
		mediaRepository.Update(media);
		var mediaDTO = mapper.Map<MediaDTO>(media);
		return mediaDTO;
	}
}