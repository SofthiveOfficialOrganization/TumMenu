using Application.Common.Base.DTOs;
using Application.Medias.DTOs;
using Application.Categories.DTOs;
using Application.MenuDesigns.DTOs;
using Application.Stores.DTOs;
using Domain.Entities;

namespace Application.Menus.DTOs;

public sealed class MenuDTO : BaseDTO
{
	public string? Title { get; set; }
	public Guid? StoreId { get; set; }
	public Guid? CompanyId { get; set; }
	public Guid? MenuDesignId { get; set; }
	public MenuDesignDTO? MenuDesign { get; set; }
	public List<MediaDTO> Medias { get; set; } = [];
	public ICollection<CategoryDTO> Categories { get; set; } = [];
	public MenuStatus Status { get; set; }
	public string? StoreName { get; set; }
	public string? CompanyName { get; set; }
	public string? StoreSlug { get; set; }
	public string? CompanySlug { get; set; }
	public string? StorePhoneNumber { get; set; }
	public string? StoreSecondaryPhoneNumber { get; set; }
	public string? StoreFullAddress { get; set; }
	public double? StoreLatitude { get; set; }
	public double? StoreLongitude { get; set; }
	public string? StoreLogoUrl { get; set; }
	public string? StoreBannerUrl { get; set; }
	public bool IsDefaultCompanyMenu { get; set; }
	public bool ShowRepresentativeImagesDisclaimer { get; set; }

	// Görünürlük kontrolleri
	public bool ShowPricesOnMenu { get; set; } = true;   // Menüde fiyatlar gösterilsin mi?
	public bool ShowMenuButton { get; set; } = true;    // Dükkan detay sayfasında menü butonu gösterilsin mi?
	public bool ShowSocialLinksOnMenu { get; set; } = false;
	public bool ShowPhoneNumberOnMenu { get; set; } = false;
	public bool ShowAddressOnMenu { get; set; } = false;
	public bool ShowCoverPhotoOnQrMenu { get; set; } = false;
	public List<StoreSocialLinkDTO> SocialLinks { get; set; } = [];
}
