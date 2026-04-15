using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.Constants;
using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions;
using WebAPI.Models.Concrete;
using WebAPI.Models.Dtos.Province;
using WebAPI.Models.Dtos.TurkeyApi;

namespace WebAPI.Services.ProvinceServices;

public interface IProvinceService
{
    /// <summary>
    /// Türkiye'deki il ve ilçe verilerini Türkiye API'sinden çekerek veri tabanına ekler.
    /// </summary>
    Task FetchTurkeyData(CancellationToken cancellationToken = default);

    /// <summary>
    /// Veri tabanındaki mevcut il ve ilçe verilerini temizler.
    /// </summary>
    Task ClearDistrictAndProvinces(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen il kimliğine (provinceId) göre il bilgilerini getirir.
    /// </summary>
    /// <param name="provinceId">İl kimliği</param>
    /// <returns>ProvinceDto</returns>
    Task<ProvinceDto> GetByProvinceIdAsync(Guid provinceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sayfalama desteğiyle il bilgilerini listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <returns>ProvinceListModel</returns>
    Task<ProvinceListModel> GetListAsync(PageRequest pageRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Veri tabanındaki tüm il bilgilerini getirir.
    /// </summary>
    /// <returns>Liste halinde ProvinceDto</returns>
    Task<List<ProvinceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sadece il bilgilerini getirir (ilçeler dahil edilmez).
    /// </summary>
    /// <returns>Liste halinde ProvinceDto</returns>
    Task<List<ProvinceDto>> GetOnlyProvincesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dinamik sorgular kullanarak il bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="dynamicQuery">Dinamik sorgu</param>
    /// <returns>ProvinceListModel</returns>
    Task<ProvinceListModel> GetListByDynamicAsync(
        PageRequest pageRequest,
        DynamicQuery dynamicQuery,
        CancellationToken cancellationToken = default);

}

public class ProvinceService(
    IHttpClientFactory httpClientFactory,
    IMapper mapper,
    IProvinceRepository provinceRepository
    ) : IProvinceService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMapper _mapper = mapper;
    private readonly IProvinceRepository _provinceRepository = provinceRepository;

    /// <summary>
    /// Türkiye'deki il ve ilçe verilerini Türkiye API'sinden çekerek veri tabanına ekler.
    /// </summary>
    public async Task FetchTurkeyData(CancellationToken cancellationToken = default)
    {
        var isProvince  = await _provinceRepository.AnyAsync(
            cancellationToken: cancellationToken);

        if (isProvince) throw new BusinessException(AppMessages.SHOULD_NOT_BE_A_REGISTERED_PROVINCE);
        
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync(requestUri: $"{ApiUrls.TURKEY_API_BASE_URL}{ApiUrls.TURKEY_API_GET_ALL_PROVINCES}", cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var provinces = JsonSerializer.Deserialize<ProvinceResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var mappedProvinces = _mapper.Map<ICollection<Province>>(provinces!.Data);
        
        await _provinceRepository.AddRangeAsync(
            entities: mappedProvinces,
            cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Veri tabanındaki mevcut il ve ilçe verilerini temizler.
    /// </summary>
    public async Task ClearDistrictAndProvinces(CancellationToken cancellationToken = default)
    {
        var provinces = await _provinceRepository.GetAllAsync(
            include: x => x.Include(x => x.Districts),
            cancellationToken: cancellationToken);
        
        if(provinces.Count == 0)
            throw new NotFoundException(AppMessages.NO_PROVINCE_FOUND_TO_DELETE);
        
        await _provinceRepository.DeleteRangeAsync(
            entities: provinces,
            permanent: true,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Belirtilen il kimliğine (provinceId) göre il bilgilerini getirir.
    /// </summary>
    /// <param name="provinceId">İl kimliği</param>
    /// <returns>ProvinceDto</returns>
    public async Task<ProvinceDto> GetByProvinceIdAsync(Guid provinceId, CancellationToken cancellationToken = default)
    {
        var province = await _provinceRepository.GetAsync(
            predicate: x => x.Id == provinceId,
            include: x => x.Include(x => x.Districts),
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        if (province == null) throw new NotFoundException(AppMessages.PROVINCE_NOT_FOUND);
        
        var mappedProvinceDto = _mapper.Map<ProvinceDto>(province);
        return mappedProvinceDto;
    }

    /// <summary>
    /// Veri tabanındaki tüm il bilgilerini getirir.
    /// </summary>
    /// <returns>Liste halinde ProvinceDto</returns>
    public async Task<List<ProvinceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var provinces = await _provinceRepository.GetAllAsync(
            include: x => x.Include(x => x.Districts),
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedProvinces = _mapper.Map<List<ProvinceDto>>(provinces);
        return mappedProvinces;
    }
    
    /// <summary>
    /// Sadece il bilgilerini getirir (ilçeler dahil edilmez).
    /// </summary>
    /// <returns>Liste halinde ProvinceDto</returns>
    public async Task<List<ProvinceDto>> GetOnlyProvincesAsync(CancellationToken cancellationToken = default)
    {
        var provinces = await _provinceRepository.GetAllAsync(
            enableTracking: false,
            cancellationToken: cancellationToken);
    
        var mappedProvinces = _mapper.Map<List<ProvinceDto>>(provinces);
        return mappedProvinces;
    }

    /// <summary>
    /// Dinamik sorgular kullanarak il bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="dynamicQuery">Dinamik sorgu</param>
    /// <returns>ProvinceListModel</returns>
    public async Task<ProvinceListModel> GetListByDynamicAsync(PageRequest pageRequest, DynamicQuery dynamicQuery, CancellationToken cancellationToken = default)
    {
        var provinces = await _provinceRepository.GetListByDynamicAsync(
            include: x => x.Include(x => x.Districts),
            dynamic: dynamicQuery,
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedProvinceListModel = _mapper.Map<ProvinceListModel>(provinces);
        return mappedProvinceListModel;
    }

    /// <summary>
    /// Sayfalama desteğiyle il bilgilerini listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <returns>ProvinceListModel</returns>
    public async Task<ProvinceListModel> GetListAsync(PageRequest pageRequest, CancellationToken cancellationToken = default)
    {
        var provinces = await _provinceRepository.GetListAsync(
            include: x => x.Include(x => x.Districts),
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedProvinceListModel = _mapper.Map<ProvinceListModel>(provinces);
        return mappedProvinceListModel;
    }
}