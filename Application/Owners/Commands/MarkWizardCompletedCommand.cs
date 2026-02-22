using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Owners.Commands;

public class MarkWizardCompletedCommand : IRequest<bool>, ITransactionalRequest
{
}

public class MarkWizardCompletedHandler(
	IRepository<Owner> repo,
	IUserContext userContext
) : IRequestHandler<MarkWizardCompletedCommand, bool>
{
	public async Task<bool> Handle(MarkWizardCompletedCommand request, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;

		var owner = await repo.Query().FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId, ct);
		if (owner == null)
			return false;

		owner.WizardCompleted = true;
		
		repo.Update(owner);
		return true;
	}
}
