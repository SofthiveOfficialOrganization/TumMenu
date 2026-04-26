using AutoMapper;
using WebAPI.DataAccess.Paging;
using WebAPI.Models.Concrete;
using WebAPI.Models.Dtos.Country;
using WebAPI.Models.Dtos.District;
using WebAPI.Models.Dtos.Province;
using WebAPI.Models.Dtos.RestCountriesApi;
using WebAPI.Models.Dtos.TurkeyApi;

namespace WebAPI.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CityDataDto, Province>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Coordinates != null ? src.Coordinates.Latitude : 0))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Coordinates != null ? src.Coordinates.Longitude : 0))
            .ForMember(dest => dest.GoogleMaps, opt => opt.MapFrom(src => src.Maps != null ? src.Maps.GoogleMaps : null))
            .ForMember(dest => dest.OpenStreetMap, opt => opt.MapFrom(src => src.Maps != null ? src.Maps.OpenStreetMap : null))
            .ForMember(dest => dest.Districts, opt => opt.MapFrom(src => src.Districts));

        CreateMap<Province, ProvinceDto>().ReverseMap();
        CreateMap<IPaginate<Province>, ProvinceListModel>().ReverseMap();

        CreateMap<DistrictData, District>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Population, opt => opt.MapFrom(src => src.Population))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<District, DistrictDto>().ReverseMap();
        CreateMap<IPaginate<District>, DistrictListModel>().ReverseMap();

        CreateMap<CountryDataDto, Country>()
            .ForMember(dest => dest.CommonName, opt => opt.MapFrom(src => src.Name != null ? src.Name.Common : null))
            .ForMember(dest => dest.OfficialName, opt => opt.MapFrom(src => src.Name != null ? src.Name.Official : null))
            .ForMember(dest => dest.Alpha2Code, opt => opt.MapFrom(src => src.Alpha2Code))
            .ForMember(dest => dest.Alpha3Code, opt => opt.MapFrom(src => src.Alpha3Code))
            .ForMember(dest => dest.NumericCode, opt => opt.MapFrom(src => src.NumericCode))
            .ForMember(dest => dest.IsIndependent, opt => opt.MapFrom(src => src.IsIndependent ?? false))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsUnMember, opt => opt.MapFrom(src => src.IsUNMember))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
            .ForMember(dest => dest.Subregion, opt => opt.MapFrom(src => src.Subregion))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Coordinates != null && src.Coordinates.Count > 0 ? src.Coordinates[0] : 0))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Coordinates != null && src.Coordinates.Count > 1 ? src.Coordinates[1] : 0))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.Population, opt => opt.MapFrom(src => src.Population))
            .ForMember(dest => dest.FlagEmoji, opt => opt.MapFrom(src => src.FlagEmoji))
            .ForMember(dest => dest.GoogleMaps, opt => opt.MapFrom(src => src.Maps != null ? src.Maps.GoogleMaps : null))
            .ForMember(dest => dest.OpenStreetMap, opt => opt.MapFrom(src => src.Maps != null ? src.Maps.OpenStreetMaps : null))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezones != null && src.Timezones.Count > 0 ? src.Timezones[0] : "UTC"))
            .ForMember(dest => dest.StartOfWeek, opt => opt.MapFrom(src => src.StartOfWeek))
            .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src =>
                src.Currencies != null ? src.Currencies.Select(kvp => new CountryCurrency
                {
                    Code = kvp.Key, Name = kvp.Value.Name, Symbol = kvp.Value.Symbol
                }).ToList() : new List<CountryCurrency>()))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src =>
                src.Languages != null ? src.Languages.Select(kvp => new CountryLanguage
                {
                    Code = kvp.Key, Name = kvp.Value
                }).ToList() : new List<CountryLanguage>()))
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src =>
                src.Name != null && src.Name.NativeName != null ? src.Name.NativeName.Select(kvp => new CountryTranslation
                {
                    LanguageCode = kvp.Key, OfficialName = kvp.Value.Official, CommonName = kvp.Value.Common
                }).ToList() : new List<CountryTranslation>()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<Country, CountryDto>().ReverseMap();
        CreateMap<Country, CountryBasicInfoDto>().ReverseMap();
        CreateMap<CountryCurrency, CountryCurrencyDto>().ReverseMap();
        CreateMap<CountryLanguage, CountryLanguageDto>().ReverseMap();
        CreateMap<CountryTranslation, CountryTranslationDto>().ReverseMap();
        CreateMap<IPaginate<Country>, CountryListModel>().ReverseMap();
    }
}
