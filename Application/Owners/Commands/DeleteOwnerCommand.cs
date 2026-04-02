using Application.Abstractions;
using Application.Common.Helpers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Owners.Commands;

public class DeleteOwnerCommand : IRequest<Unit>, ITransactionalRequest
{
	public Guid OwnerId { get; set; }
}

public class DeleteOwnerCommandHandler(
	IRepository<Domain.Entities.Owner> repoOwner
) : IRequestHandler<DeleteOwnerCommand, Unit>
{
	public async Task<Unit> Handle(DeleteOwnerCommand req, CancellationToken ct)
	{
		var owner = await repoOwner.GetByIdAsync(req.OwnerId, ct).EnsureFound("Silinecek yönetici bulunamadı");
		repoOwner.SoftDelete(owner!);
		return Unit.Value;
	}
}
