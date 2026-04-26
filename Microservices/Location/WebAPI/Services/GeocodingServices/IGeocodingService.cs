using System.Text.Json;
using System.Text.Json.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.Dtos.Geocoding;

namespace WebAPI.Services.GeocodingServices;

public interface IGeocodingService
{
    Task<List<GeocodingResultDto>> SearchAsync(string query, int limit = 5, CancellationToken ct = default);
    Task<GeocodingResultDto?> ReverseAsync(double lat, double lon, CancellationToken ct = default);
}

public class GeocodingService(IHttpClientFactory httpClientFactory) : IGeocodingService
{
    private const string BaseUrl = "https://nominatim.openstreetmap.org";

    public async Task<List<GeocodingResultDto>> SearchAsync(string query, int limit = 5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new BusinessException("Arama sorgusu boş olamaz.");

        var client = CreateClient(httpClientFactory);
        var url = $"{BaseUrl}/search?q={Uri.EscapeDataString(query)}&format=json&limit={limit}&countrycodes=tr&addressdetails=1";

        var response = await client.GetStringAsync(url, ct);
        var results = JsonSerializer.Deserialize<List<NominatimResult>>(response, JsonOptions);

        return results?.Select(Map).ToList() ?? [];
    }

    public async Task<GeocodingResultDto?> ReverseAsync(double lat, double lon, CancellationToken ct = default)
    {
        var client = CreateClient(httpClientFactory);
        var url = $"{BaseUrl}/reverse?lat={lat}&lon={lon}&format=json";

        var response = await client.GetStringAsync(url, ct);
        var result = JsonSerializer.Deserialize<NominatimResult>(response, JsonOptions);

        return result is null ? null : Map(result);
    }

    private static HttpClient CreateClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        // Nominatim kullanım koşulu: User-Agent zorunlu
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TumMenuLocationService/1.0 (destek@tummenu.com)");
        return client;
    }

    private static GeocodingResultDto Map(NominatimResult r)
    {
        double.TryParse(r.Lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat);
        double.TryParse(r.Lon, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon);

        BoundingBoxDto? bbox = null;
        if (r.Boundingbox?.Count == 4 &&
            double.TryParse(r.Boundingbox[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var minLat) &&
            double.TryParse(r.Boundingbox[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var maxLat) &&
            double.TryParse(r.Boundingbox[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var minLon) &&
            double.TryParse(r.Boundingbox[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var maxLon))
        {
            bbox = new BoundingBoxDto(minLat, maxLat, minLon, maxLon);
        }

        return new GeocodingResultDto(r.DisplayName ?? string.Empty, lat, lon, r.Type, bbox);
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class NominatimResult
    {
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
        [JsonPropertyName("lat")] public string? Lat { get; set; }
        [JsonPropertyName("lon")] public string? Lon { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("boundingbox")] public List<string>? Boundingbox { get; set; }
    }
}
