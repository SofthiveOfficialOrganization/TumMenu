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

public class UpdateMediaCommand : IRequest<MediaDTO>, ITransactionalRequest
{
	public Guid Id { get; set; }
	public string MediaUrl { get; set; } = string.Empty;
	public string? AltText { get; set; }
	public int SortOrder { get; set; }
	public string? Slot { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public long? FileSize { get; set; }
	public string? Extension { get; set; }
	public string? MimeType { get; set; }
	public Guid ReferenceId { get; set; }
	public MediaRefType MediaRefType { get; set; }
}

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