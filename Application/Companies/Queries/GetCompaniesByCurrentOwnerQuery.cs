using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Companies.Queries;

public record GetCompaniesByCurrentOwnerQuery() : IRequest<List<CompanyDTO>>;

public class GetCompaniesByCurrentOwnerHandler(
	IRepository<Domain.Entities.Company> repoCompany,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetCompaniesByCurrentOwnerQuery, List<CompanyDTO>>
{
	public async Task<List<CompanyDTO>> Handle(GetCompaniesByCurrentOwnerQuery req, CancellationToken ct)
	{
		var ownerId = userContext.UserId;
		var companyList = await repoCompany.Query()
			.Where(c => c.Owner != null && c.Owner.ApplicationUserId == ownerId)
			.Include(c => c.BaseMenu)
			.Include(c => c.Subscription)
			.Include(c => c.PaymentMethods)
			.Include(c => c.Stores)
			.ToListAsync(ct);
		if(companyList.Count == 0)
			throw new NotFoundAppException("Kullanıcının hiç şirketi bulunamadı.");

		var companyListDTO = mapper.Map<List<CompanyDTO>>(companyList);
		return companyListDTO;
	}
}
