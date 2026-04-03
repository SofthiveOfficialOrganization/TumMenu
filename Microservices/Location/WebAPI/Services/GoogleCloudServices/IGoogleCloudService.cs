using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebAPI.Constants;
using WebAPI.Exceptions;
using WebAPI.Models.Concrete;
using WebAPI.Models.Dtos.Aws;
using WebAPI.Models.Dtos.GoogleCloud;

namespace WebAPI.Services.GoogleCloudServices;

public interface IGoogleCloudService
{
    /// <summary>
    /// Google Maps API kullanarak verilen adres bilgisinden enlem ve boylam değerlerini alır.
    /// </summary>
    /// <param name="address">Adres bilgisi</param>
    /// <returns>Konum verisi (enlem ve boylam)</returns>
    /// <exception cref="NotFoundException">Eğer sonuç bulunamazsa hata fırlatır</exception>
    Task<LocationData?> GetCoordinatesAsync(string address);

    Task<List<AddressSuggestionDto>> GetAddressSuggestionsAsync(string query);
}

public class GoogleCloudService(
    IOptions<GoogleCloudOptions> options,
    HttpClient httpClient
    ) : IGoogleCloudService
{
    private readonly GoogleCloudOptions _googleCloudOptions = options.Value;
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Google Maps API kullanarak verilen adres bilgisinden enlem ve boylam değerlerini alır.
    /// </summary>
    /// <param name="address">Adres bilgisi</param>
    /// <returns>Konum verisi (enlem ve boylam)</returns>
    /// <exception cref="NotFoundException">Eğer sonuç bulunamazsa hata fırlatır</exception>
    public async Task<LocationData?> GetCoordinatesAsync(string address)
    {
        var requestUri = string.Format(ApiUrls.GOOGLE_CLOUD_GEOCODE_API, address, _googleCloudOptions.ApiKey);

        var response = await _httpClient.GetStringAsync(requestUri);
        dynamic? jsonResponse = JsonConvert.DeserializeObject(response);

        if (jsonResponse is null)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        dynamic results = jsonResponse!.results;
        if (results is null || results.Count == 0)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        var result = results[0];
        var latitude = result?.geometry?.location?.lat ?? 0;
        var longitude = result?.geometry?.location?.lng ?? 0;

        return new LocationData
        {
            Label = address,
            Latitude = latitude,
            Longitude = longitude
        };
    }

    /// <summary>
    /// Google Places API kullanarak verilen metin girdisine göre önerilen adresleri listeler.
    /// </summary>
    /// <param name="query">Kullanıcı tarafından girilen adres metni</param>
    /// <returns>Önerilen adres listesi</returns>
    /// <exception cref="NotFoundException">Eğer sonuç bulunamazsa hata fırlatır</exception>
    public async Task<List<AddressSuggestionDto>> GetAddressSuggestionsAsync(string query)
    {
        var requestUri = string.Format(ApiUrls.GOOGLE_CLOUD_AUTOCOMPLETE_API, query, _googleCloudOptions.ApiKey);

        var response = await _httpClient.GetStringAsync(requestUri);
        dynamic? jsonResponse = JsonConvert.DeserializeObject(response);

        if (jsonResponse is null)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        dynamic predictions = jsonResponse!.predictions;
        if (predictions is null)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        var suggestions = ((IEnumerable<dynamic>)predictions)
            .Select(p => new AddressSuggestionDto
            {
                SuggestedAddress = (string?)p.description,
                SecondaryText = p.structured_formatting?.secondary_text != null
                    ? (string)p.structured_formatting.secondary_text
                    : string.Empty
            })
            .ToList();

        return suggestions;
    }

}
