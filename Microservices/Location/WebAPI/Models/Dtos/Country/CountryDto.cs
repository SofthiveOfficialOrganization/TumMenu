namespace WebAPI.Models.Dtos.Country;

public record CountryDto(
    Guid Id, 
    string CommonName
    // string OfficialName,
    // string Alpha2Code,
    // string Alpha3Code,
    // string? NumericCode,
    // bool IsIndependent,
    // string Status,
    // bool IsUnMember,
    // string Region,
    // string? Subregion,
    // double? Latitude, 
    // double? Longitude,
    // double? Area,
    // long? Population,
    // string? FlagEmoji,
    // string? GoogleMaps,
    // string? OpenStreetMap,
    // string? Timezone,
    // string? StartOfWeek,
    // List<CountryCurrencyDto>? Currencies,
    // List<CountryLanguageDto>? Languages,
    // List<CountryTranslationDto>? Translations
    );