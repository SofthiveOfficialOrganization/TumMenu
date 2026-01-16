using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Companies.DTOs;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public class GetCompanyByIdQuery : IRequest<CompanyDTO>
{
	public Guid Id { get; set; }
}

public class GetCompanyByIdHandler(
	IRepository<Domain.Entities.Company> repoCompany,
	IMapper mapper
) : IRequestHandler<GetCompanyByIdQuery, CompanyDTO>
{
	public async Task<CompanyDTO> Handle(GetCompanyByIdQuery req, CancellationToken ct)
	{
		var company = (await repoCompany.Query()
			.Where(c => c.Id == req.Id)
			.Include(c => c.BaseMenu)
			.Include(c => c.Subscription)
			.Include(c => c.PaymentMethods)
			.Include(c => c.Stores)
			.FirstOrDefaultAsync(ct)).EnsureFound("Şirket bulunamadı.");
		var companyDTO = mapper.Map<CompanyDTO>(company);
		return companyDTO;
	}
}
