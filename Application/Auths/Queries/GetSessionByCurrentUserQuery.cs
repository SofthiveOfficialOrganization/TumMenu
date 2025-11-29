using Application.Abstractions;
using Application.Auths.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Queries;

public sealed record GetSessionByCurrentUserQuery : IRequest<SessionDTO>;

public sealed class GetSesssionByCurrentUserHandler(
    IUserContext userContext,
    IRepository<Company> repoCompany,
    IRepository<Store> repoStore
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

        Guid? ownerId = Guid.TryParse(userContext.OwnerId, out var owid) ? owid : null;


        return new SessionDTO()
        {
            UserId = userContext.UserId ?? "",
            Email = userContext.Email ?? "",
            Roles = userContext.Roles,
            OwnerId = ownerId
        };
    }
}