using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public class UpdateCompanyCommand : IRequest<CompanyDTO>, ITransactionalRequest
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	private string _slug = string.Empty;
	public string Slug { get => _slug; set => _slug = value ?? string.Empty; }
}

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
		if(req.Slug != company.Slug)
		{
			var slugExists = await repoCompany.Query().AnyAsync(c => c.Slug == req.Slug, ct);
			if(slugExists)
				throw new AlreadyExistsAppException("Bu slug zaten kullanılıyor");
		}
		mapper.Map(req, company);
		repoCompany.Update(company);
		var companyDTO = mapper.Map<CompanyDTO>(company);
		return companyDTO;
	}
}
