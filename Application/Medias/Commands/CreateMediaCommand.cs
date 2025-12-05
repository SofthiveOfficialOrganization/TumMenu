using Application.Abstractions;
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

public sealed record CreateMediaCommand(
	string MediaUrl,
	string? AltText,
	int? SortOrder,
	string? Slot,
	int? Width,
	int? Height,
	Guid ReferenceId,
	MediaRefType MediaRefType
) : IRequest<MediaDTO>, ITransactionalRequest;

public class CreateMediaCommandHandler(
	IRepository<Media> mediaRepository,
	IMapper mapper
	) : IRequestHandler<CreateMediaCommand, MediaDTO>
{
	public async Task<MediaDTO> Handle(CreateMediaCommand req, CancellationToken ct)
	{
		var media = mapper.Map<Media>(req);
		await mediaRepository.AddAsync(media, ct);
		return mapper.Map<MediaDTO>(media);
	}
}