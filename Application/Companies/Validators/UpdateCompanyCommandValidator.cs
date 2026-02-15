using Application.Companies.Commands;
using FluentValidation;

namespace Application.Companies.Validators;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
	public UpdateCompanyCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Şirket adı boş olamaz.")
			.MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir.");

		RuleFor(x => x.Slug)
			.NotEmpty().WithMessage("Slug boş olamaz.")
			.MaximumLength(30).WithMessage("Slug en fazla 30 karakter olabilir.");
	}
}
