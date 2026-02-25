using AutoMapper;
using WebAPI.Constants;
using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions;
using WebAPI.Models.Dtos.District;

namespace WebAPI.Services.DistrictServices;

public interface IDistrictService
{
    /// <summary>
    /// Sayfalama desteğiyle ilçe bilgilerini listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictListModel</returns>
    Task<DistrictListModel> GetListAsync(PageRequest pageRequest, CancellationToken cancellationToken = default);
    /// <summary>
    /// Belirtilen il kimliğine (provinceId) göre ilçeleri listeler.
    /// </summary>
    /// <param name="provinceId">İl kimliği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictDto listesi</returns>
    Task<List<DistrictDto>> GetListByProvinceIdAsync(Guid provinceId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Veri tabanındaki tüm ilçe bilgilerini getirir.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictDto listesi</returns>
    Task<List<DistrictDto>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Dinamik sorgular kullanılarak ilçe bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="dynamicQuery">Dinamik sorgu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictListModel</returns>
    Task<DistrictListModel> GetListByDynamicAsync(
        PageRequest pageRequest, 
        DynamicQuery dynamicQuery,
        CancellationToken cancellationToken = default);
}

public class DistrictService(
    IHttpClientFactory httpClientFactory,
    IMapper mapper,
    IDistrictRepository districtRepository
    ) : IDistrictService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IDistrictRepository _districtRepository = districtRepository;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Sayfalama desteğiyle ilçe bilgilerini listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictListModel</returns>
    public async Task<DistrictListModel> GetListAsync(PageRequest pageRequest, CancellationToken cancellationToken = default)
    {
        var districts = await _districtRepository.GetListAsync(
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedDistrictListModel = _mapper.Map<DistrictListModel>(districts);
        return mappedDistrictListModel;
    }

    /// <summary>
    /// Belirtilen il kimliğine (provinceId) göre ilçeleri listeler.
    /// </summary>
    /// <param name="provinceId">İl kimliği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictDto listesi</returns>
    public async Task<List<DistrictDto>> GetListByProvinceIdAsync(Guid provinceId, CancellationToken cancellationToken = default)
    {
        var districts = await _districtRepository.GetAllAsync(
            predicate: x => x.ProvinceId == provinceId,
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        if (districts == null) throw new NotFoundException(AppMessages.PROVINCE_NOT_FOUND);
        
        var mappedDistricts = _mapper.Map<List<DistrictDto>>(districts);
        return mappedDistricts;
    }

    /// <summary>
    /// Veri tabanındaki tüm ilçe bilgilerini getirir.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictDto listesi</returns>
    public async Task<List<DistrictDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var districts = await _districtRepository.GetAllAsync(
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedDistricts = _mapper.Map<List<DistrictDto>>(districts);
        return mappedDistricts;
    }

    /// <summary>
    /// Dinamik sorgular kullanılarak ilçe bilgilerini filtreleme ve sıralama yaparak listeler.
    /// </summary>
    /// <param name="pageRequest">Sayfalama isteği</param>
    /// <param name="dynamicQuery">Dinamik sorgu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>DistrictListModel</returns>
    public async Task<DistrictListModel> GetListByDynamicAsync(
        PageRequest pageRequest, 
        DynamicQuery dynamicQuery,
        CancellationToken cancellationToken = default)
    {
        var districts = await _districtRepository.GetListByDynamicAsync(
            index: pageRequest.PageIndex,
            size: pageRequest.PageIndex,
            dynamic: dynamicQuery,
            enableTracking: false,
            cancellationToken: cancellationToken);
        
        var mappedDistrictListModel = _mapper.Map<DistrictListModel>(districts);
        return mappedDistrictListModel;
    }
}