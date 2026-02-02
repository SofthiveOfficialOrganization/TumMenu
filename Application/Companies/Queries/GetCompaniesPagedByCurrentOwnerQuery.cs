using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Companies.DTOs;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public class GetCompaniesPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<CompanyDTO>>
{
}

public class GetCompaniesPagedByCurrentOwnerHandler(
	IRepository<Domain.Entities.Company> repoCompany,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetCompaniesPagedByCurrentOwnerQuery, PaginatedListDTO<CompanyDTO>>
{
	public async Task<PaginatedListDTO<CompanyDTO>> Handle(GetCompaniesPagedByCurrentOwnerQuery req, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;

		var companies = await repoCompany.GetPageListAsync(
			req,
			c => c.Owner != null && c.Owner.ApplicationUserId == applicationUserId,
			include: q => q
				.Include(c => c.BaseMenu)
				.Include(c => c.Subscription)
				.Include(c => c.PaymentMethods)
				.Include(c => c.Stores),
			orderBy: q => q.OrderBy(c => c.Title),
			enableTracking: false,
			ct: ct
		);

		var companyListDto = mapper.Map<PaginatedListDTO<CompanyDTO>>(companies);
		return companyListDto;
	}
}
