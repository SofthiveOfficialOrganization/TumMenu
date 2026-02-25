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
    private readonly HttpClient _httpClient = new HttpClient();

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
        dynamic jsonResponse = JsonConvert.DeserializeObject(response);

        if (jsonResponse == null || jsonResponse.predictions == null)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        var latitude = jsonResponse.results[0].geometry.location.lat;
        var longitude = jsonResponse.results[0].geometry.location.lng;

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

        var response = await httpClient.GetStringAsync(requestUri);
        dynamic jsonResponse = JsonConvert.DeserializeObject(response);

        if (jsonResponse == null || jsonResponse.predictions == null)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);

        // predictions listesini IEnumerable<dynamic> olarak cast ediyoruz.
        IEnumerable<dynamic> predictions = jsonResponse.predictions;

        var suggestions = predictions
            .Select(p => new AddressSuggestionDto
            {
                SuggestedAddress = (string)p.description,
                SecondaryText = p.structured_formatting.secondary_text != null 
                    ? (string)p.structured_formatting.secondary_text 
                    : string.Empty
            })
            .ToList();

        return suggestions;
    }

}
