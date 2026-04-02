using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Companies.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Companies.Queries;

public class GetCompanyListForSearchQuery : PageRequest, IRequest<PaginatedListDTO<CompanyFilterDTO>>, IAuthorizedRequest
{
    public string? SearchTerm { get; set; }
}

public class GetCompanyListForSearchHandler(
    IRepository<Company> repoCompany,
    IUserContext userContext
) : IRequestHandler<GetCompanyListForSearchQuery, PaginatedListDTO<CompanyFilterDTO>>
{
    public async Task<PaginatedListDTO<CompanyFilterDTO>> Handle(GetCompanyListForSearchQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var isAdmin = userContext.IsAdmin;

        var query = repoCompany.Query();

        if (!isAdmin)
        {
            query = query.Where(c => c.Owner != null && c.Owner.ApplicationUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        // PageRequest.Page is typically 0-based in this project architecture based on previous observation (PaginatedListDTOBase.Index)
        // Ensure PageSize is valid
        if (request.PageSize <= 0) request.PageSize = 10;

        var items = await query
            .OrderBy(c => c.Title)
            .Skip((request.Page - request.From) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CompanyFilterDTO(c.Id, c.Title))
            .ToListAsync(cancellationToken);

        return new PaginatedListDTO<CompanyFilterDTO>
        {
            Items = items,
            Index = request.Page,
            Size = request.PageSize,
            From = request.From,
            Count = totalCount,
            Pages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            HasPrevious = request.Page > request.From,
            HasNext = request.Page < (int)Math.Ceiling(totalCount / (double)request.PageSize) + request.From - 1
        };
    }
}
