using Amazon.LocationService.Model;
using AutoMapper;
using WebAPI.DataAccess.Paging;
using WebAPI.Models.Concrete;
using WebAPI.Models.Dtos.Aws;
using WebAPI.Models.Dtos.Country;
using WebAPI.Models.Dtos.District;
using WebAPI.Models.Dtos.Province;
using WebAPI.Models.Dtos.RestCountriesApi;
using WebAPI.Models.Dtos.TurkeyApi;
using DistrictDto = WebAPI.Models.Dtos.District.DistrictDto;

namespace WebAPI.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Province Mapping Procedures
        
        CreateMap<CityDataDto, Province>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Coordinates.Latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Coordinates.Longitude))
            .ForMember(dest => dest.GoogleMaps, opt => opt.MapFrom(src => src.Maps.GoogleMaps))
            .ForMember(dest => dest.OpenStreetMap, opt => opt.MapFrom(src => src.Maps.OpenStreetMap))
            .ForMember(dest => dest.Districts, opt => opt.MapFrom(src => src.Districts));

        CreateMap<Province, ProvinceDto>().ReverseMap();
        CreateMap<IPaginate<Province>, ProvinceListModel>().ReverseMap();
        
        #endregion
        
        #region District Mapping Procedures
        
        CreateMap<DistrictData, District>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Population, opt => opt.MapFrom(src => src.Population))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));
        
        CreateMap<District, DistrictDto>().ReverseMap();
        CreateMap<IPaginate<District>, DistrictListModel>().ReverseMap();
        #endregion
        
        #region Country Mapping Procedures
        
        CreateMap<CountryDataDto, Country>()
            .ForMember(dest => dest.CommonName, opt => opt.MapFrom(src => src.Name.Common))
            .ForMember(dest => dest.OfficialName, opt => opt.MapFrom(src => src.Name.Official))
            .ForMember(dest => dest.Alpha2Code, opt => opt.MapFrom(src => src.Alpha2Code))
            .ForMember(dest => dest.Alpha3Code, opt => opt.MapFrom(src => src.Alpha3Code))
            .ForMember(dest => dest.NumericCode, opt => opt.MapFrom(src => src.NumericCode))
            .ForMember(dest => dest.IsIndependent, opt => opt.MapFrom(src => src.IsIndependent ?? false)) // Null kontrolü
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsUnMember, opt => opt.MapFrom(src => src.IsUNMember))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
            .ForMember(dest => dest.Subregion, opt => opt.MapFrom(src => src.Subregion))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Coordinates.Count > 0 ? src.Coordinates[0] : 0))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Coordinates.Count > 1 ? src.Coordinates[1] : 0))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.Population, opt => opt.MapFrom(src => src.Population))
            .ForMember(dest => dest.FlagEmoji, opt => opt.MapFrom(src => src.FlagEmoji))
            .ForMember(dest => dest.GoogleMaps, opt => opt.MapFrom(src => src.Maps.GoogleMaps))
            .ForMember(dest => dest.OpenStreetMap, opt => opt.MapFrom(src => src.Maps.OpenStreetMaps))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezones.Count > 0 ? src.Timezones[0] : "UTC"))
            .ForMember(dest => dest.StartOfWeek, opt => opt.MapFrom(src => src.StartOfWeek))
            .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src => 
                src.Currencies != null ? src.Currencies.Select(kvp => new CountryCurrency
                {
                    Code = kvp.Key,
                    Name = kvp.Value.Name,
                    Symbol = kvp.Value.Symbol
                }).ToList() : new List<CountryCurrency>()))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => 
                src.Languages != null ? src.Languages.Select(kvp => new CountryLanguage
                {
                    Code = kvp.Key,
                    Name = kvp.Value
                }).ToList() : new List<CountryLanguage>()))
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => 
                src.Name.NativeName != null ? src.Name.NativeName.Select(kvp => new CountryTranslation
                {
                    LanguageCode = kvp.Key,
                    OfficialName = kvp.Value.Official,
                    CommonName = kvp.Value.Common
                }).ToList() : new List<CountryTranslation>()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));


        CreateMap<Country, CountryDto>()
            // .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src => src.Currencies))
            // .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            // .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ReverseMap();
        
        CreateMap<Country, CountryBasicInfoDto>()
            // .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src => src.Currencies))
            // .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ReverseMap();
        
        CreateMap<CountryCurrency, CountryCurrencyDto>().ReverseMap();
        CreateMap<CountryLanguage, CountryLanguageDto>().ReverseMap();
        CreateMap<CountryTranslation, CountryTranslationDto>().ReverseMap();

        CreateMap<CountryCurrency, CountryCurrencyDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ReverseMap();

        CreateMap<CountryLanguage, CountryLanguageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();

        CreateMap<CountryTranslation, CountryTranslationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.OfficialName, opt => opt.MapFrom(src => src.OfficialName ?? ""))
            .ForMember(dest => dest.CommonName, opt => opt.MapFrom(src => src.CommonName ?? ""))
            .ReverseMap();

        CreateMap<IPaginate<Country>, CountryListModel>().ReverseMap();
        #endregion

        
        #region AWS Mapping Procedures

        CreateMap<SearchForTextResult, LocationData>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Place.Label))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Place.Country))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Place.Region))
            .ForMember(dest => dest.SubRegion, opt => opt.MapFrom(src => src.Place.SubRegion))
            .ForMember(dest => dest.Municipality, opt => opt.MapFrom(src => src.Place.Municipality))
            .ForMember(dest => dest.SubMunicipality, opt => opt.MapFrom(src => src.Place.SubMunicipality))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Place.Geometry.Point[1]))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Place.Geometry.Point[0]))
            .ReverseMap();

        CreateMap<SearchForSuggestionsResult, AwsAddressSuggestionDto>()
            .ForMember(dest => dest.SuggestedAddress, opt => opt.MapFrom(src => src.Text))
            .ReverseMap();
        #endregion

    }
}