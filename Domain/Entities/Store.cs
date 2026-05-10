using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Store : BaseEntity
	{
		[MaxLength(200)]
		public string Title { get; set; } = null!;
		[MaxLength(50)]
		public string Slug { get; set; } = null!;
		public string PhoneNumber { get; set; } = null!;
		[MaxLength(50)]
		public string? SecondaryPhoneNumber { get; set; }
		public bool ShowRepresentativeImagesDisclaimer { get; set; }

		// Görünürlük kontrolleri
		public bool ShowInSearchAndListings { get; set; } = true;  // Restoran aramalarda ve listelerde görünsün mü?
		public bool ShowMenuButton { get; set; } = true;         // Dükkan detay sayfasında menüye git butonu gösterilsin mi?
		public bool ShowPricesOnMenu { get; set; } = true;       // Menüde fiyatlar gösterilsin mi? (isQr=true hariç)

		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;
		public Address? Address { get; set; }
		public QRCode? QRCode { get; set; }
		public ICollection<Menu> Menus { get; set; } = [];
		public ICollection<Staff> Staffs { get; set; } = [];
		public ICollection<Media> Medias { get; set; } = [];
	}
}