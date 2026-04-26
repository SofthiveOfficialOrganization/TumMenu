namespace WebAPI.Models.Dtos.Geocoding;

public record GeocodingResultDto(
    string DisplayName,
    double Latitude,
    double Longitude,
    string? Type,
    BoundingBoxDto? BoundingBox
) : IDto;

public record BoundingBoxDto(
    double MinLat,
    double MaxLat,
    double MinLon,
    double MaxLon
);
