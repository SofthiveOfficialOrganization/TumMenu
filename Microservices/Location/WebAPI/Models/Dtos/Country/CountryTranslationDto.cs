namespace WebAPI.Models.Dtos.Country;

public record CountryTranslationDto(
    Guid Id, 
    Guid CountryId, 
    string LanguageCode, 
    string? OfficialName,
    string? CommonName
);