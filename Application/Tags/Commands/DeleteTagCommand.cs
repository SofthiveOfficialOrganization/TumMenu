using Application.Abstractions;
using Application.Common.Helpers;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tags.Commands;

public sealed record DeleteTagCommand(
	Guid Id
) : IRequest<Unit>, ITransactionalRequest;

public class DeleteTagCommandHandler(
	IRepository<Tag> repoTag
) : IRequestHandler<DeleteTagCommand, Unit>
{
	public async Task<Unit> Handle(DeleteTagCommand req, CancellationToken ct)
	{
		var tag = await repoTag.GetByIdAsync(req.Id, ct).EnsureFound("Etiket bulunamadı");
		repoTag.SoftDelete(tag!);
		return Unit.Value;
	}
}