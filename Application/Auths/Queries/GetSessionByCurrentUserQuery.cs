using Application.Abstractions;
using Application.Auths.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Queries;

public sealed record GetSessionByCurrentUserQuery : IRequest<SessionDTO>, IAuthorizedRequest;

public sealed class GetSesssionByCurrentUserHandler(
    IUserContext userContext,
    IRepository<Company> repoCompany
) : IRequestHandler<GetSessionByCurrentUserQuery, SessionDTO>
{
    public async Task<SessionDTO> Handle(GetSessionByCurrentUserQuery req, CancellationToken ct)
    {
        string? companyName = null;

        if (userContext.CompanyIdParsed.HasValue)
        {
            companyName = await repoCompany.Query()
                .Where(c => c.Id == userContext.CompanyIdParsed.Value)
                .Select(c => c.Title)
                .FirstOrDefaultAsync(ct);
        }

        return new SessionDTO()
        {
            UserId = userContext.UserId ?? "",
            Email = userContext.Email ?? "",
            Roles = userContext.Roles,
            OwnerId = userContext.OwnerIdParsed
        };
    }
}
