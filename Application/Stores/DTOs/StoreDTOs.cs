using Application.Addresses.DTOs;
using Application.Common.Base.DTOs;
using Application.Companies.DTOs;
using Application.Medias.DTOs;
using Application.Staffs.DTOs;
using Domain.Entities;

namespace Application.Stores.DTOs;

public sealed class StoreLiteDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
}

public sealed class StoreDTO : BaseDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string PhoneNumber { get; set; } = null!;
	public string? SecondaryPhoneNumber { get; set; }
	public bool ShowRepresentativeImagesDisclaimer { get; set; }

	// Görünürlük kontrolleri
	public bool ShowInSearchAndListings { get; set; } = true;
	public bool ShowMenuButton { get; set; } = true;
	public bool ShowPricesOnMenu { get; set; } = true;

	public Guid CompanyId { get; set; }
	public CompanyDTO? Company { get; set; }
	public AddressDTO Address { get; set; } = new();
	public List<StaffDTO> Staffs { get; set; } = new List<StaffDTO>();
	public List<MediaDTO> Medias { get; set; } = [];
}

