using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.Constants;
using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions;
using WebAPI.Models.Concrete;
using WebAPI.Models.Dtos.Country;
using WebAPI.Models.Dtos.RestCountriesApi;

namespace WebAPI.Services.CountryServices;

/// <summary>
/// Ülke verilerini yöneten servis arayüzü.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// RestCountries API'den ülke verilerini çekerek veritabanına ekler.
    /// </summary>
    Task FetchCountriesData(CancellationToken cancellationToken = default);

    /// <summary>
    /// Veritabanındaki tüm ülke verilerini temizler.
    /// </summary>
    Task ClearCountries(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm ülkeleri getirir.
    /// </summary>
    Task<List<CountryDto>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tüm ülkelerin temel bilgilerini getirir.
    /// </summary>
    Task<List<CountryBasicInfoDto>> GetAllCountriesBasicInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sayfalama desteğiyle ülke bilgilerini listeler.
    /// </summary>
    Task<CountryListModel> GetListAsync(PageRequest pageRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dinamik sorgular kullanarak ülke bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    Task<CountryListModel> GetListByDynamicAsync(PageRequest pageRequest, DynamicQuery dynamicQuery,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Ülke verilerini yönetmek için servis sınıfı.
/// </summary>
public class CountryService(
    IHttpClientFactory httpClientFactory,
    IMapper mapper,
    ICountryRepository countryRepository
    ) : ICountryService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMapper _mapper = mapper;
    private readonly ICountryRepository _countryRepository = countryRepository;

    /// <summary>
    /// RestCountries API'den ülke verilerini çekerek veritabanına ekler.
    /// </summary>
    public async Task FetchCountriesData(CancellationToken cancellationToken = default)
    {
        var isCountryExist = await _countryRepository.AnyAsync(cancellationToken: cancellationToken);
        if (isCountryExist) throw new BusinessException(AppMessages.SHOULD_NOT_BE_A_REGISTERED_COUNTRY);

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{ApiUrls.REST_COUNTRIES_API_BASE_URL}{ApiUrls.REST_COUNTRIES_API_GET_ALL_COUNTRIES}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Ülke verileri alınırken bir hata oluştu. Lütfen daha sonra tekrar deneyin. (Hata kodu: {response.StatusCode})");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var countryDataList = JsonSerializer.Deserialize<List<CountryDataDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (countryDataList == null || !countryDataList.Any())
            throw new BusinessException(AppMessages.COUNTRIES_DATA_NOT_FOUND);

        var mappedCountries = _mapper.Map<ICollection<Country>>(countryDataList);
        await _countryRepository.AddRangeAsync(entities: mappedCountries, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Veritabanındaki mevcut ülke verilerini temizler.
    /// </summary>
    public async Task ClearCountries(CancellationToken cancellationToken = default)
    {
         var countries = await _countryRepository.GetAllAsync(cancellationToken: cancellationToken);
        
        if (!countries.Any())
            throw new NotFoundException(AppMessages.NO_COUNTRY_FOUND_TO_DELETE);
        
        await _countryRepository.DeleteRangeAsync(entities: countries, permanent: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Tüm ülkeleri getirir.
    /// </summary>
    public async Task<List<CountryDto>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _countryRepository.GetAllAsync(
            // include: x => x.Include(x => x.Currencies).Include(x => x.Languages).Include(x => x.Translations),
            enableTracking: false, 
            cancellationToken: cancellationToken);
        
        var mappedCountries = _mapper.Map<List<CountryDto>>(countries);
        return mappedCountries;
    }
    
    /// <summary>
    /// Tüm ülkelerin temel bilgilerini getirir.
    /// </summary>
    public async Task<List<CountryBasicInfoDto>> GetAllCountriesBasicInfoAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _countryRepository.GetAllAsync(
            // include: x => x.Include(x => x.Currencies).Include(x => x.Translations),
            enableTracking: false, 
            cancellationToken: cancellationToken);

        var mappedCountries = _mapper.Map<List<CountryBasicInfoDto>>(countries);
        return mappedCountries;
    }
    
    /// <summary>
    /// Sayfalama desteğiyle ülke bilgilerini listeler.
    /// </summary>
    public async Task<CountryListModel> GetListAsync(PageRequest pageRequest, CancellationToken cancellationToken = default)
    {
        var countries = await _countryRepository.GetListAsync(
            // include: x => x.Include(x => x.Currencies).Include(x => x.Languages).Include(x => x.Translations),
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);
    
        var mappedCountryListModel = _mapper.Map<CountryListModel>(countries);
        return mappedCountryListModel;
    }
    
    /// <summary>
    /// Dinamik sorgular kullanarak ülke bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    public async Task<CountryListModel> GetListByDynamicAsync(PageRequest pageRequest, DynamicQuery dynamicQuery, CancellationToken cancellationToken = default)
    {
        var countries = await _countryRepository.GetListByDynamicAsync(
            // include: x => x.Include(x => x.Currencies).Include(x => x.Languages).Include(x => x.Translations),
            dynamic: dynamicQuery,
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        var mappedCountryListModel = _mapper.Map<CountryListModel>(countries);
        return mappedCountryListModel;
    }
}
