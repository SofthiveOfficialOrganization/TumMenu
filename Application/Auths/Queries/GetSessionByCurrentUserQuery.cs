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
        Guid? storeId = null;
        string? storeName = null;

        if(Guid.TryParse(userContext.CompanyId, out var cid))
        {
            companyId = cid;
            companyName = await repoCompany.Query()
                .Where(c => c.Id == cid)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);
        }
        if(Guid.TryParse(userContext.StoreId, out var sid))
        {
            storeId = sid;
            storeName = await repoStore.Query()
                    .Where(s => s.Id == sid)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync(ct);
        }

        Guid? ownerId = Guid.TryParse(userContext.OwnerId, out var owid) ? owid : null;
        Guid? staffId = Guid.TryParse(userContext.StaffId, out var stid) ? stid : null;


        return new SessionDTO(
            userContext.UserId ?? "",
            userContext.Email ?? "",
            Roles: userContext.Roles,
            OwnerId: ownerId,
            StaffId: staffId,
            CompanyId: companyId,
            CompanyName: companyName,
            StoreId: storeId,
            StoreName: storeName
        );
    }
}