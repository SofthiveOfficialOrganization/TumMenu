using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Owners.Queries;

public class GetWizardStatusQuery : IRequest<bool>
{
}

public class GetWizardStatusHandler(
	IRepository<Owner> repo,
	IUserContext userContext
) : IRequestHandler<GetWizardStatusQuery, bool>
{
	public async Task<bool> Handle(GetWizardStatusQuery request, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;

		var owner = await repo.Query().FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId, ct);
		if (owner == null)
			return false;

		return owner.WizardCompleted;
	}
}
