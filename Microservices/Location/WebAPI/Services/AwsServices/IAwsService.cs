using Amazon;
using Amazon.LocationService;
using Amazon.LocationService.Model;
using AutoMapper;
using Microsoft.Extensions.Options;
using WebAPI.Constants;
using WebAPI.Exceptions;
using WebAPI.Models.Dtos.Aws;
using AwsAddressSuggestionDto = WebAPI.Models.Dtos.Aws.AwsAddressSuggestionDto;
using AwsOptions = WebAPI.Models.Concrete.AwsOptions;

namespace WebAPI.Services.AwsServices;

public interface IAwsService
{
    /// <summary>
    /// Verilen adres bilgisine göre coğrafi koordinatları getirir.
    /// </summary>
    /// <param name="address">Adres bilgisi</param>
    /// <returns>Konum verisi</returns>
    Task<List<LocationData>?> GetCoordinatesAsync(string address);
    /// <summary>
    /// Kullanıcının girdiği adres bilgisine göre önerilen adresleri listeler.
    /// </summary>
    /// <param name="query">Adres önerisi için girilen metin</param>
    /// <returns>Önerilen adres listesi</returns>
    Task<List<AwsAddressSuggestionDto>> GetAddressSuggestionsAsync(string query);
}

public class AwsService : IAwsService
{
    private readonly AmazonLocationServiceClient _locationClient;
    private readonly AwsOptions _awsOptions;
    private readonly IMapper _mapper;

    public AwsService(IOptions<AwsOptions> options, IMapper mapper)
    {
        _mapper = mapper;
        _awsOptions = options.Value;
        _locationClient = new AmazonLocationServiceClient(
            _awsOptions.AccessKey,
            _awsOptions.SecretKey,
            RegionEndpoint.GetBySystemName(_awsOptions.Region)
        );
    }

    /// <summary>
    /// Verilen adres bilgisine göre coğrafi koordinatları getirir.
    /// </summary>
    /// <param name="address">Adres bilgisi</param>
    /// <returns>Konum verisi</returns>
    public async Task<List<LocationData>?> GetCoordinatesAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
        {
            throw new BusinessException(AppMessages.INVALID_ADDRESS);
        }

        
        var request = new SearchPlaceIndexForTextRequest
        {
            IndexName = _awsOptions.PlaceIndex, 
            Text = address
            
        };
        var response = await _locationClient.SearchPlaceIndexForTextAsync(request);

        if (response.Results.Count == 0)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);
        
        var mappedLocationData = _mapper.Map<List<LocationData>>(response.Results);
        return mappedLocationData;
    }

    /// <summary>
    /// Kullanıcının girdiği adres bilgisine göre önerilen adresleri listeler.
    /// </summary>
    /// <param name="query">Adres önerisi için girilen metin</param>
    /// <returns>Önerilen adres listesi</returns>
    public async Task<List<AwsAddressSuggestionDto>> GetAddressSuggestionsAsync(string query)
    {
        var request = new SearchPlaceIndexForSuggestionsRequest
        {
            IndexName = _awsOptions.PlaceIndex, 
            Text = query
        };

        var response = await _locationClient.SearchPlaceIndexForSuggestionsAsync(request);

        if (response.Results.Count == 0)
            throw new NotFoundException(AppMessages.LOCATION_NOT_FOUND);
        
        var mappedAwsAddressSuggestionDto = _mapper.Map<List<AwsAddressSuggestionDto>>(response.Results);
        return mappedAwsAddressSuggestionDto;
    }
}