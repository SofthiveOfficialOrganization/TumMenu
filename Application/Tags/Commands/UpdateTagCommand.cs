using Application.Abstractions;
using Application.Common.Helpers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags.Commands;

public sealed record UpdateTagCommand(
	Guid Id,
	string Name
) : IRequest<Guid>, ITransactionalRequest;

public class UpdateTagCommandHandler(
	IRepository<Domain.Entities.Tag> repoTag
) : IRequestHandler<UpdateTagCommand, Guid>
{
	public async Task<Guid> Handle(UpdateTagCommand req, CancellationToken ct)
	{
		var tag = await repoTag.GetByIdAsync(req.Id, ct).EnsureFound("Etiket bulunamadı");
		tag!.Name = req.Name;
		repoTag.Update(tag);
		return tag.Id;
	}
}