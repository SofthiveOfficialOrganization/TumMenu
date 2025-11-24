using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public record UpdateCompanyCommand(Guid Id, string Name, string Slug) : IRequest<CompanyDTO>, ITransactionalRequest;

public class UpdateCompanyHandler(
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<UpdateCompanyCommand, CompanyDTO>
{
	public async Task<CompanyDTO> Handle(UpdateCompanyCommand req, CancellationToken ct)
	{
		var company = await repoCompany.Query().FirstOrDefaultAsync(c => c.Id == req.Id, ct);
		if(company == null)
			throw new NotFoundAppException("Şirket bulunamadı");
		mapper.Map(req, company);
		var companyDTO = mapper.Map<CompanyDTO>(company);
		return companyDTO;
	}
}
