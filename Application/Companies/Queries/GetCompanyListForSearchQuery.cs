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

        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var page = Math.Max(request.Page, request.From);

        var items = await query
            .OrderBy(c => c.Title)
            .Skip((page - request.From) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyFilterDTO(c.Id, c.Title))
            .ToListAsync(cancellationToken);

        var pages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedListDTO<CompanyFilterDTO>
        {
            Items = items,
            Index = page,
            Size = pageSize,
            From = request.From,
            Count = totalCount,
            Pages = pages,
            HasPrevious = page > request.From,
            HasNext = page - request.From + 1 < pages
        };
    }
}
