using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Medias.Commands;

public class DeleteMediaCommand : IRequest<Unit>
{
	public Guid Id { get; set; }
}

public class DeleteMediaCommandHandler(
	IRepository<Media> mediaRepository
	) : IRequestHandler<DeleteMediaCommand, Unit>
{
	public async Task<Unit> Handle(DeleteMediaCommand req, CancellationToken ct)
	{
		var media = await mediaRepository.GetByIdAsync(req.Id, ct).EnsureFound("Medya içeriği bulunamadı");
		mediaRepository.SoftDelete(media);
		return Unit.Value;
	}
}