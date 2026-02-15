using Application.Abstractions;
using Application.Companies.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public class GetCompanyByCurrentOwnerQuery : IRequest<CompanyDTO?>
{
}

public class GetCompanyByCurrentOwnerHandler(
	IRepository<Company> repoCompany,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetCompanyByCurrentOwnerQuery, CompanyDTO?>
{
	public async Task<CompanyDTO?> Handle(GetCompanyByCurrentOwnerQuery req, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;

		var company = await repoCompany.Query()
			.Include(c => c.Owner)
			.FirstOrDefaultAsync(c => c.Owner != null && c.Owner.ApplicationUserId == applicationUserId, ct);

		if (company == null)
			return null;

		return mapper.Map<CompanyDTO>(company);
	}
}
