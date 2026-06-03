using Application.Abstractions;
using Application.Addresses.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Stores.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Commands;

public sealed class UpdateStoreCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string PhoneNumber { get; set; } = string.Empty;
	public string? SecondaryPhoneNumber { get; set; }
	public bool ShowRepresentativeImagesDisclaimer { get; set; }

	// Görünürlük kontrolleri
	public bool ShowInSearchAndListings { get; set; }
	public bool ShowMenuButton { get; set; }
	public bool ShowPricesOnMenu { get; set; }
	public bool ShowSocialLinksOnMenu { get; set; }
	public bool ShowPhoneNumberOnMenu { get; set; }
	public bool ShowAddressOnMenu { get; set; }
	public bool ShowCoverPhotoOnQrMenu { get; set; }

	public List<StoreSocialLinkDTO> SocialLinks { get; set; } = [];
	public AddressDTO? Address { get; set; }

	public string ActionName => "Dükkan güncellendi";
	public Guid EntityId => Id;
}

public sealed class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
	public UpdateStoreCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Dükkan bulunamadı.");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Dükkan adı boş olamaz.")
			.MaximumLength(200).WithMessage("Dükkan adı en fazla 200 karakter olabilir.");

		RuleFor(x => x.Slug)
			.NotEmpty().WithMessage("Dükkan slug'ı boş olamaz.")
			.MaximumLength(30).WithMessage("Dükkan slug'ı en fazla 30 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Dükkan slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.");

		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithMessage("Telefon numarası boş olamaz.");
	}
}

public class UpdateStoreCommandHandler(
	IRepository<Store> repoStore,
	IApplicationDbContext db,
	IMapper mapper
) : IRequestHandler<UpdateStoreCommand, Unit>
{
	public async Task<Unit> Handle(UpdateStoreCommand req, CancellationToken ct)
	{
		var store = await repoStore.Query(tracked: true)
					.Include(s => s.Address)
					.FirstOrDefaultAsync(s => s.Id == req.Id, ct) ?? throw new NotFoundAppException("Dükkan bulunamadı.");

		var slug = req.Slug?.Trim();
		if(string.IsNullOrWhiteSpace(slug))
			throw new ValidationAppException(new Dictionary<string, string[]>
			{
				[nameof(req.Slug)] = ["Dükkan slug'ı boş olamaz."]
			});

		if(slug != store.Slug)
		{
			var slugExistsInCompany = await repoStore.Query()
				.AnyAsync(s => s.CompanyId == store.CompanyId && s.Slug == slug && s.Id != req.Id, ct);
			if(slugExistsInCompany)
				throw new AlreadyExistsAppException("Bu slug zaten kullanılmakta.");
		}

		store.Title = req.Title;
		store.Slug = slug;
		store.PhoneNumber = req.PhoneNumber;
		store.SecondaryPhoneNumber = req.SecondaryPhoneNumber;
		store.ShowRepresentativeImagesDisclaimer = req.ShowRepresentativeImagesDisclaimer;
		store.ShowInSearchAndListings = req.ShowInSearchAndListings;
		store.ShowMenuButton = req.ShowMenuButton;
		store.ShowPricesOnMenu = req.ShowPricesOnMenu;
		store.ShowSocialLinksOnMenu = req.ShowSocialLinksOnMenu;
		store.ShowPhoneNumberOnMenu = req.ShowPhoneNumberOnMenu;
		store.ShowAddressOnMenu = req.ShowAddressOnMenu;
		store.ShowCoverPhotoOnQrMenu = req.ShowCoverPhotoOnQrMenu;

		if(req.Address is not null)
		{
			if(store.Address is null)
			{
				store.Address = mapper.Map<Address>(req.Address);
			}
			else
			{
				mapper.Map(req.Address, store.Address);
			}
		}

		await StoreSocialLinkSync.ApplyAsync(db, store.Id, req.SocialLinks, ct);
		return Unit.Value;
	}
}

internal static class StoreSocialLinkSync
{
	public static async Task ApplyAsync(
		IApplicationDbContext db,
		Guid storeId,
		IReadOnlyCollection<StoreSocialLinkDTO>? links,
		CancellationToken ct)
	{
		var normalizedLinks = Normalize(links);
		var existingLinks = await db.StoreSocialLinks
			.Where(link => link.StoreId == storeId)
			.ToListAsync(ct);

		var submittedExistingIds = normalizedLinks
			.Where(link => link.Id != Guid.Empty)
			.Select(link => link.Id)
			.ToHashSet();

		var removedLinks = existingLinks
			.Where(link => !submittedExistingIds.Contains(link.Id))
			.ToList();

		foreach(var removedLink in removedLinks)
		{
			removedLink.IsDeleted = true;
			removedLink.DeletedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
		}

		foreach(var link in normalizedLinks)
		{
			var existingLink = link.Id == Guid.Empty
				? null
				: existingLinks.FirstOrDefault(existing => existing.Id == link.Id);

			if(existingLink is null)
			{
				db.StoreSocialLinks.Add(new StoreSocialLink
				{
					Id = Guid.NewGuid(),
					StoreId = storeId,
					Platform = link.Platform,
					DisplayName = link.DisplayName,
					Url = link.Url,
					SortOrder = link.SortOrder
				});
				continue;
			}

			existingLink.Platform = link.Platform;
			existingLink.DisplayName = link.DisplayName;
			existingLink.Url = link.Url;
			existingLink.SortOrder = link.SortOrder;
		}
	}

	public static void Apply(Store store, IReadOnlyCollection<StoreSocialLinkDTO>? links)
	{
		var normalizedLinks = Normalize(links);

		var submittedExistingIds = normalizedLinks
			.Where(link => link.Id != Guid.Empty)
			.Select(link => link.Id)
			.ToHashSet();

		var removedLinks = store.SocialLinks
			.Where(link => !submittedExistingIds.Contains(link.Id))
			.ToList();

		foreach(var removedLink in removedLinks)
		{
			store.SocialLinks.Remove(removedLink);
		}

		foreach(var link in normalizedLinks)
		{
			var existingLink = link.Id == Guid.Empty
				? null
				: store.SocialLinks.FirstOrDefault(existing => existing.Id == link.Id);

			if(existingLink is null)
			{
				store.SocialLinks.Add(new StoreSocialLink
				{
					Id = Guid.NewGuid(),
					StoreId = store.Id,
					Platform = link.Platform,
					DisplayName = link.DisplayName,
					Url = link.Url,
					SortOrder = link.SortOrder
				});
				continue;
			}

			existingLink.Platform = link.Platform;
			existingLink.DisplayName = link.DisplayName;
			existingLink.Url = link.Url;
			existingLink.SortOrder = link.SortOrder;
		}
	}

	private sealed record NormalizedSocialLink(
		Guid Id,
		StoreSocialPlatform Platform,
		string? DisplayName,
		string Url,
		int SortOrder);

	private static List<NormalizedSocialLink> Normalize(IReadOnlyCollection<StoreSocialLinkDTO>? links)
	{
		return (links ?? [])
			.Where(l => !string.IsNullOrWhiteSpace(l.Url))
			.Select(link => new
			{
				Source = link,
				Url = NormalizeUrl(link.Url)
			})
			.Where(link => link.Url is not null)
			.Select((link, index) => new NormalizedSocialLink(
				link.Source.Id,
				Enum.IsDefined(typeof(StoreSocialPlatform), link.Source.Platform)
					? link.Source.Platform
					: StoreSocialPlatform.Other,
				string.IsNullOrWhiteSpace(link.Source.DisplayName)
					? null
					: link.Source.DisplayName.Trim(),
				link.Url!,
				index))
			.ToList();
	}

	private static string? NormalizeUrl(string url)
	{
		var trimmedUrl = url.Trim();
		if(!trimmedUrl.Contains("://", StringComparison.Ordinal))
		{
			trimmedUrl = $"https://{trimmedUrl}";
		}

		if(!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri) ||
			(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
		{
			return null;
		}

		return uri.ToString();
	}
}
