using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public class CreateCompanyCommand : IRequest<CompanyDTO>, ITransactionalRequest, IAuditableCommand
{
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string ActionName => "Şirket oluşturuldu";
}

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
	public CreateCompanyCommandValidator()
	{
		RuleFor(c => c.Title)
			.NotEmpty().WithMessage("Şirket adı boş olamaz.")
			.MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir.");
		RuleFor(c => c.Slug)
			.MaximumLength(30).WithMessage("Şirket slug'ı en fazla 30 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
				.When(c => !string.IsNullOrWhiteSpace(c.Slug));
	}
}
public class CreateCompanyCommandHandler(
	IRepository<Company> repoCompany,
	IRepository<Owner> repoOwner,
	IMapper mapper,
	IUserContext userContext
) : IRequestHandler<CreateCompanyCommand, CompanyDTO>
{
	public async Task<CompanyDTO> Handle(CreateCompanyCommand req, CancellationToken ct)
	{
		var owner = await repoOwner.Query().Where(o => o.ApplicationUserId == userContext.UserId).FirstOrDefaultAsync(ct);
		if(owner == null)
			throw new NotFoundAppException("Hesabınıza ait bir işletme sahibi kaydı bulunamadı.");

		var existingCompany = await repoCompany.Query()
			.FirstOrDefaultAsync(c => c.OwnerId == owner.Id, ct);

		if(existingCompany != null)
		{
			throw new AlreadyExistsAppException("Bu hesap zaten bir şirkete sahip. Birden fazla şirket oluşturulamaz.");
		}

		var slug = req.Slug ?? SlugHelper.Slugify(req.Title);

		var slugCollision = await repoCompany.Query()
			.FirstOrDefaultAsync(c => c.Slug == slug, ct);

		if(slugCollision != null)
		{
			throw new AlreadyExistsAppException($"'{slug}' slug'ına sahip bir şirket bulunmakta. Farklı bir slug değeri girin.");
		}

		var company = mapper.Map<Company>(req);
		company.Slug = slug;
		company.OwnerId = owner.Id;
		await repoCompany.AddAsync(company, ct);
		var companyDTO = mapper.Map<CompanyDTO>(company);
		return companyDTO;
	}
}
