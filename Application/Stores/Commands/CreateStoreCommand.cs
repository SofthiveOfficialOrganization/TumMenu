using Application.Abstractions;
using Application.Addresses.DTOs;
using Application.Common.Exceptions;
using Application.Menus.Commands;
using Application.Stores.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Commands;

public class CreateStoreCommand : IRequest<StoreDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Dükkan oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string PhoneNumber { get; set; } = string.Empty;
	public string? SecondaryPhoneNumber { get; set; }
	public bool ShowRepresentativeImagesDisclaimer { get; set; }

	// Görünürlük kontrolleri
	public bool ShowInSearchAndListings { get; set; } = false;
	public bool ShowMenuButton { get; set; } = false;
	public bool ShowPricesOnMenu { get; set; } = false;
	public bool ShowSocialLinksOnMenu { get; set; } = false;
	public bool ShowPhoneNumberOnMenu { get; set; } = false;
	public bool ShowAddressOnMenu { get; set; } = false;
	public bool ShowCoverPhotoOnQrMenu { get; set; } = false;

	public Guid CompanyId { get; set; }
	public AddressDTO? Address { get; set; }
	public List<StoreSocialLinkDTO> SocialLinks { get; set; } = [];
}

public sealed class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
	public CreateStoreCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Dükkan adı boş olamaz.")
			.MaximumLength(200).WithMessage("Dükkan adı en fazla 200 karakter olabilir.");

		RuleFor(x => x.Slug)
			.MaximumLength(30).WithMessage("Dükkan slug'ı en fazla 30 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Dükkan slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
			.When(x => !string.IsNullOrWhiteSpace(x.Slug));

		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithMessage("Telefon numarası boş olamaz.");

		RuleFor(x => x.CompanyId)
			.NotEmpty().WithMessage("Şirket seçilmelidir.");
	}
}

public class CreateStoreCommandHandler(
	IRepository<Store> repoStore,
	IMapper mapper,
	IRepository<Company> repoCompany,
    IMediator mediator
) : IRequestHandler<CreateStoreCommand, StoreDTO>
{
	public async Task<StoreDTO> Handle(CreateStoreCommand req, CancellationToken ct)
	{
		var company = await repoCompany.Query()
			.FirstOrDefaultAsync(c => c.Id == req.CompanyId, ct);
		if(company is null)
			throw new UnprocessableAppException("Dükkanın ekleneceği şirket bulunamadı.");

		var slug = string.IsNullOrWhiteSpace(req.Slug)
			? SlugHelper.Slugify(req.Title)
			: req.Slug.Trim();

		if(string.IsNullOrWhiteSpace(slug))
			throw new ValidationAppException(new Dictionary<string, string[]>
			{
				[nameof(req.Slug)] = ["Dükkan slug'ı oluşturulamadı. Lütfen geçerli bir dükkan adı veya slug girin."]
			});

		var slugExistsInCompany = await repoStore.Query()
			.AnyAsync(s => s.CompanyId == req.CompanyId && s.Slug == slug, ct);
		if(slugExistsInCompany)
			throw new AlreadyExistsAppException("Bu slug zaten kullanılmakta.");

		var store = mapper.Map<Store>(req);
		if (store.Id == Guid.Empty)
			store.Id = Guid.NewGuid();

		store.Slug = slug;
		StoreSocialLinkSync.Apply(store, req.SocialLinks);
		await repoStore.AddAsync(store, ct);

        // Generate QR Code automatically
        await mediator.Send(new Application.QRs.Commands.GenerateQRCodeCommand { StoreId = store.Id }, ct);

		if (company.DefaultMainMenuId.HasValue)
		{
			await mediator.Send(new SyncMainMenuToStoreMenuCommand
			{
				SourceMenuId = company.DefaultMainMenuId.Value,
				StoreId = store.Id
			}, ct);
		}
		else
		{
			await mediator.Send(new CreateMenuToStoreCommand
			{
				Title = $"{store.Title} Menü",
				StoreId = store.Id,
				Status = MenuStatus.Active
			}, ct);
		}
        
        var storeDTO = mapper.Map<StoreDTO>(store);
		return storeDTO;
	}
}
