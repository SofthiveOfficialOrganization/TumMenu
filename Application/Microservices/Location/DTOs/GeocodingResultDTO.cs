namespace Application.Microservices.Location.DTOs
{
    public class GeocodingResultDTO
    {
        public string DisplayName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Type { get; set; }
        public BoundingBoxDTO? BoundingBox { get; set; }
    }

    public class BoundingBoxDTO
    {
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLon { get; set; }
    }
}
