namespace WebAPI.Models.Dtos.Country;

public record CountryLanguageDto(
    Guid Id, 
    Guid CountryId, 
    string Code,
    string Name
);