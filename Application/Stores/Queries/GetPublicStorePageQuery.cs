using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Stores.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed record GetPublicStorePageQuery(string CompanySlug, string StoreSlug) : IRequest<PublicStorePageDTO>;

public sealed class PublicStorePageDTO
{
	public string StoreTitle { get; init; } = null!;
	public string CompanyTitle { get; init; } = null!;
	public string CompanySlug { get; init; } = null!;
	public string StoreSlug { get; init; } = null!;
	public string? LogoUrl { get; init; }
	public string? BannerUrl { get; init; }
	public IReadOnlyList<PublicStoreGalleryImageDTO> GalleryImages { get; init; } = [];
	public string PhoneNumber { get; init; } = null!;
	public string? SecondaryPhoneNumber { get; init; }
	public string? FullAddress { get; init; }
	public double? Latitude { get; init; }
	public double? Longitude { get; init; }
	public bool MenuAvailable { get; init; }
	public bool ShowMenuButton { get; init; }           // Menüye git butonu gösterilsin mi?
	public bool ShowPricesOnMenu { get; init; }         // Menüde fiyatlar gösterilsin mi?
	public bool ShowSocialLinksOnMenu { get; init; }
	public bool ShowCoverPhotoOnQrMenu { get; init; }
	public bool ShowRepresentativeImagesDisclaimer { get; init; }
	public IReadOnlyList<StoreSocialLinkDTO> SocialLinks { get; init; } = [];
}

public sealed class PublicStoreGalleryImageDTO
{
	public string Url { get; init; } = null!;
	public string? Alt { get; init; }
}

public sealed class GetPublicStorePageQueryHandler(
	IRepository<Store> repoStore,
	IRepository<Menu> repoMenu
) : IRequestHandler<GetPublicStorePageQuery, PublicStorePageDTO>
{
	public async Task<PublicStorePageDTO> Handle(GetPublicStorePageQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query(tracked: false)
			.Include(s => s.Company)
			.Include(s => s.Address)
			.Include(s => s.Menus)
			.Include(s => s.Medias)
			.Include(s => s.SocialLinks)
			.FirstOrDefaultAsync(
				s => s.Slug == req.StoreSlug && s.Company.Slug == req.CompanySlug && !s.IsDeleted,
				ct);

		// Dükkan bulunamadı veya aramalarda/listelerde gizli - ama URL ile erişilebilir
		// Bu sayfada sadece silinmiş dükkanları engelliyoruz, ShowInSearchAndListings burada önemli değil
		if (store is null)
			throw new NotFoundAppException("Dükkan bulunamadı.");

		var imageMedias = store.Medias
			.Where(m => !m.IsDeleted && m.Kind == MediaKind.Image)
			.ToList();

		var bannerUrl = imageMedias.FirstOrDefault(m => m.Slot == "banner")?.MediaUrl;
		var logoUrl = imageMedias.FirstOrDefault(m => m.Slot == "logo")?.MediaUrl;
		var gallery = imageMedias
			.Where(m => m.Slot == "default-gallery")
			.OrderBy(m => m.SortOrder)
			.Select(m => new PublicStoreGalleryImageDTO
			{
				Url = m.MediaUrl,
				Alt = m.AltText ?? store.Title
			})
			.ToList();

		var menuAvailable = await ResolveMenuAvailableAsync(store, ct);

		return new PublicStorePageDTO
		{
			StoreTitle = store.Title,
			CompanyTitle = store.Company.Title,
			CompanySlug = store.Company.Slug,
			StoreSlug = store.Slug,
			LogoUrl = logoUrl,
			BannerUrl = bannerUrl,
			GalleryImages = gallery,
			PhoneNumber = store.PhoneNumber,
			SecondaryPhoneNumber = store.SecondaryPhoneNumber,
			FullAddress = store.Address?.FullAddress,
			Latitude = store.Address?.Latitude,
			Longitude = store.Address?.Longitude,
			MenuAvailable = menuAvailable,
			ShowMenuButton = store.ShowMenuButton,
			ShowPricesOnMenu = store.ShowPricesOnMenu,
			ShowSocialLinksOnMenu = store.ShowSocialLinksOnMenu,
			ShowCoverPhotoOnQrMenu = store.ShowCoverPhotoOnQrMenu,
			ShowRepresentativeImagesDisclaimer = store.ShowRepresentativeImagesDisclaimer,
			SocialLinks = store.SocialLinks
				.Where(sl => !string.IsNullOrWhiteSpace(sl.Url))
				.OrderBy(sl => sl.SortOrder)
				.Select(sl => new StoreSocialLinkDTO
				{
					Id = sl.Id,
					Platform = sl.Platform,
					DisplayName = sl.DisplayName,
					Url = sl.Url,
					SortOrder = sl.SortOrder
				})
				.ToList()
		};
	}

	private async Task<bool> ResolveMenuAvailableAsync(Store store, CancellationToken ct)
	{
		if (store.Menus.Any(m => m.Status == MenuStatus.Active))
			return true;

		if (store.Company.DefaultMainMenuId.HasValue)
		{
			var ok = await repoMenu.Query(tracked: false).AnyAsync(
				m => m.Id == store.Company.DefaultMainMenuId.Value &&
					m.CompanyId == store.Company.Id &&
					m.Status == MenuStatus.MainMenu,
				ct);
			if (ok)
				return true;
		}

		return await repoMenu.Query(tracked: false).AnyAsync(
			m => m.CompanyId == store.Company.Id && m.Status == MenuStatus.MainMenu,
			ct);
	}
}
