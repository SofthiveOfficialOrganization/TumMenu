namespace Application.Microservices.Location.DTOs
{
	public class GetProvinceResponseBasicDTO
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string GoogleMaps { get; set; } = null!;
		public string OpenStreetMap { get; set; } = null!;
	}
}
