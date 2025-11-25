using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Companies.Queries;

public record GetAllCompaniesPagedQuery() : PageRequest, IRequest<PaginatedListDTO<CompanyDTO>>;

public class GetAllCompaniesPagedHandler(
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<GetAllCompaniesPagedQuery, PaginatedListDTO<CompanyDTO>>
{
	public async Task<PaginatedListDTO<CompanyDTO>> Handle(GetAllCompaniesPagedQuery req, CancellationToken ct)
	{
		var companies = repoCompany.GetPageListAsync(
			req,
			orderBy: q => q.OrderBy(c => c.Name),
			ct: ct
		);
		if(companies is null)
			throw new NotFoundAppException("Sistemde hiç şirket bulunamadı");
		var companyDTOs = mapper.Map<PaginatedListDTO<CompanyDTO>>(await companies);
		return companyDTOs;
	}
}