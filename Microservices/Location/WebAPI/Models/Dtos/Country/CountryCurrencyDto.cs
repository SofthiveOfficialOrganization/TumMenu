namespace WebAPI.Models.Dtos.Country;

public record CountryCurrencyDto(
    Guid Id, 
    Guid CountryId, 
    string Code,
    string Name,
    string? Symbol
);