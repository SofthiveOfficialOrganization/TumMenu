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

public class CreateMediaCommand : IRequest<MediaDTO>, ITransactionalRequest
{
	public string MediaUrl { get; set; } = string.Empty;
	public string? AltText { get; set; }
	public int? SortOrder { get; set; }
	public string? Slot { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public long? FileSize { get; set; }
	public string? Extension { get; set; }
	public string? MimeType { get; set; }
	public Guid ReferenceId { get; set; }
	public MediaRefType MediaRefType { get; set; }
}

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