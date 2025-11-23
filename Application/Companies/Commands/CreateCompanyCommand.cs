using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public record CreateCompanyCommand(string Name, string? Slug) : IRequest<CompanyDTO>, ITransactionalRequest;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
	public CreateCompanyCommandValidator()
	{
		RuleFor(c => c.Name)
			.NotEmpty().WithMessage("Şirket adı boş olamaz.")
			.MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir.");
		RuleFor(c => c.Slug)
			.MaximumLength(100).WithMessage("Şirket slug'ı en fazla 100 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
				.When(c => !string.IsNullOrWhiteSpace(c.Slug));
	}
}
public class CreateCompanyCommandHandler(
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<CreateCompanyCommand, CompanyDTO>
{
	public async Task<CompanyDTO> Handle(CreateCompanyCommand req, CancellationToken ct)
	{
		var exists = false;
		if(req.Slug is not null)
			exists = await repoCompany.Query().AnyAsync(c => c.Slug == req.Slug, ct);
		else
			exists = await repoCompany.Query().AnyAsync(c => c.Slug == SlugHelper.Slugify(req.Name), ct);
		if(exists)
			throw new AlreadyExistsAppException($"'{req.Slug}' slug'ına sahip bir şirket bulunmakta. Farklı bir slug değeri girin.");

		var company = mapper.Map<Company>(req);
		await repoCompany.AddAsync(company, ct);
		var companyDTO = mapper.Map<CompanyDTO>(company);
		return companyDTO;
	}
}
