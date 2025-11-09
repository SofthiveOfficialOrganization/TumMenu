using Application.Abstractions;
using Application.Auths.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auths.Queries;

public sealed record GetSessionByCurrentUserQuery : IRequest<SessionDTO>;

public sealed class GetSesssionByCurrentUserHandler(
	IUserContext userContext,
	IRepository<Company> repoCompany
) : IRequestHandler<GetSessionByCurrentUserQuery, SessionDTO>
{
	public async Task<SessionDTO> Handle(GetSessionByCurrentUserQuery req, CancellationToken ct)
	{
		Guid? companyId = null;
		string? companyName = null;

		if(Guid.TryParse(userContext.CompanyId, out var cid))
		{
			companyId = cid;
			companyName = await repoCompany.Query()
				.Where(c => c.Id == cid)
				.Select(c => c.Name)
				.FirstOrDefaultAsync(ct);
		}

		Guid? ownerId = Guid.TryParse(userContext.OwnerId, out var oid) ? oid : null;
		Guid? staffId = Guid.TryParse(userContext.StaffId, out var sid) ? sid : null;

		return new SessionDTO(
			userContext.UserId ?? "",
			userContext.Email ?? "",
			Roles: userContext.Roles,
			OwnerId: ownerId,
			StaffId: staffId,
			CompanyId: companyId,
			CompanyName: companyName
		);
	}
}