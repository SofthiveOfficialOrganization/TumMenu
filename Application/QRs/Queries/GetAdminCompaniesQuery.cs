using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetAdminCompaniesQuery : IRequest<List<AdminCompanyDTO>>, IAuthorizedRequest;

public class AdminCompanyDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
}

public class GetAdminCompaniesQueryHandler(
    IRepository<Company> repoCompany,
    IUserContext userContext
) : IRequestHandler<GetAdminCompaniesQuery, List<AdminCompanyDTO>>
{
    public async Task<List<AdminCompanyDTO>> Handle(GetAdminCompaniesQuery req, CancellationToken ct)
    {
        if (!userContext.IsAdmin)
            return new List<AdminCompanyDTO>();

        return await repoCompany.Query(tracked: false)
            .OrderBy(c => c.Title)
            .Select(c => new AdminCompanyDTO
            {
                Id = c.Id,
                Title = c.Title
            })
            .ToListAsync(ct);
    }
}
