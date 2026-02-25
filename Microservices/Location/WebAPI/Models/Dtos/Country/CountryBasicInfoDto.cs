namespace WebAPI.Models.Dtos.Country;

public record CountryBasicInfoDto(
    Guid Id, 
    string CommonName
    // string OfficialName,
    // string Alpha2Code,
    // string Alpha3Code,
    // string? NumericCode,
    // double? Latitude, 
    // double? Longitude,
    // string? GoogleMaps,
    // string? OpenStreetMap,
    // string? Timezone,
    // List<CountryCurrencyDto>? Currencies,
    // List<CountryTranslationDto>? Translations
);