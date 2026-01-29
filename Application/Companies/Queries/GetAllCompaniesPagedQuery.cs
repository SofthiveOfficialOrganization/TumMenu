using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Companies.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Companies.Queries;

public class GetAllCompaniesPagedQuery : PageRequest, IRequest<PaginatedListDTO<CompanyDTO>>
{
	public string? Search { get; set; }
}

public class GetAllCompaniesPagedHandler(
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<GetAllCompaniesPagedQuery, PaginatedListDTO<CompanyDTO>>
{
	public async Task<PaginatedListDTO<CompanyDTO>> Handle(GetAllCompaniesPagedQuery req, CancellationToken ct)
	{
		var companies = await repoCompany.GetPageListAsync(
			req,
			c =>
				string.IsNullOrEmpty(req.Search) ||
				c.Name.Contains(req.Search) ||
				c.Slug.Contains(req.Search),
			orderBy: q => q.OrderBy(c => c.Name),
			ct: ct
		);
		var companyDTOs = mapper.Map<PaginatedListDTO<CompanyDTO>>(companies);
		return companyDTOs;
	}
}