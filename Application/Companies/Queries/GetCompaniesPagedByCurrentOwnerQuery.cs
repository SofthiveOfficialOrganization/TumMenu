using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public record GetCompaniesPagedByCurrentOwnerQuery() : PageRequest, IRequest<PaginatedListDTO<CompanyDTO>>;

public class GetCompaniesPagedByCurrentOwnerHandler(
	IRepository<Domain.Entities.Company> repoCompany,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetCompaniesPagedByCurrentOwnerQuery, PaginatedListDTO<CompanyDTO>>
{
	public async Task<PaginatedListDTO<CompanyDTO>> Handle(GetCompaniesPagedByCurrentOwnerQuery req, CancellationToken ct)
	{
		var ownerId = userContext.UserId;

		var companies = await repoCompany.GetPageListAsync(
			req,
			c => c.Owner != null && c.Owner.ApplicationUserId == ownerId,
			include: q => q
				.Include(c => c.BaseMenu)
				.Include(c => c.Subscription)
				.Include(c => c.PaymentMethods)
				.Include(c => c.Stores),
			orderBy: q => q.OrderBy(c => c.Name),
			enableTracking: false,
			ct: ct
		);

		if(companies.Count == 0)
			throw new NotFoundAppException("Kullanıcının hiç şirketi bulunamadı.");

		var companyListDto = mapper.Map<PaginatedListDTO<CompanyDTO>>(companies);
		return companyListDto;
	}
}
