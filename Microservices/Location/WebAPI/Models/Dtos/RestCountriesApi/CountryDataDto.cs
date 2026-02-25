using System.Text.Json.Serialization;

namespace WebAPI.Models.Dtos.RestCountriesApi;

public class CountryDataDto
{
    [JsonPropertyName("name")]
    public CountryNameDataDto Name { get; set; }

    [JsonPropertyName("tld")]
    public List<string> TopLevelDomain { get; set; }

    [JsonPropertyName("cca2")]
    public string Alpha2Code { get; set; }

    [JsonPropertyName("ccn3")]
    public string? NumericCode { get; set; }

    [JsonPropertyName("cca3")]
    public string Alpha3Code { get; set; }

    [JsonPropertyName("independent")]
    public bool? IsIndependent { get; set; } // Bazı ülkelerde `null` olabilir

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("unMember")]
    public bool IsUNMember { get; set; }

    [JsonPropertyName("currencies")]
    public Dictionary<string, CurrencyDetailDataDto> Currencies { get; set; }

    [JsonPropertyName("idd")]
    public InternationalDialingCodeDataDto DialingCode { get; set; }

    [JsonPropertyName("capital")]
    public List<string> Capitals { get; set; }

    [JsonPropertyName("altSpellings")]
    public List<string> AlternativeSpellings { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    [JsonPropertyName("subregion")]
    public string? Subregion { get; set; }

    [JsonPropertyName("languages")]
    public Dictionary<string, string> Languages { get; set; }

    [JsonPropertyName("translations")]
    public Dictionary<string, CountryTranslationDataDto> Translations { get; set; }

    [JsonPropertyName("latlng")]
    public List<double> Coordinates { get; set; }

    [JsonPropertyName("landlocked")]
    public bool IsLandlocked { get; set; }

    [JsonPropertyName("area")]
    public double? Area { get; set; }

    [JsonPropertyName("demonyms")]
    public Dictionary<string, DemonymDetailDataDto> Demonyms { get; set; }

    [JsonPropertyName("flag")]
    public string? FlagEmoji { get; set; }

    [JsonPropertyName("maps")]
    public CountryMapsDataDto Maps { get; set; }

    [JsonPropertyName("population")]
    public int? Population { get; set; }

    [JsonPropertyName("car")]
    public VehicleInfoDataDto Vehicle { get; set; }

    [JsonPropertyName("timezones")]
    public List<string>? Timezones { get; set; }

    [JsonPropertyName("continents")]
    public List<string> Continents { get; set; }

    [JsonPropertyName("flags")]
    public CountryFlagsDataDto Flags { get; set; }

    [JsonPropertyName("coatOfArms")]
    public CoatOfArmsDataDto CoatOfArms { get; set; }

    [JsonPropertyName("startOfWeek")]
    public string? StartOfWeek { get; set; }

    [JsonPropertyName("capitalInfo")]
    public CapitalCoordinatesDataDto CapitalInfo { get; set; }

    [JsonPropertyName("cioc")]
    public string NationalOlympicCommittee { get; set; }

    [JsonPropertyName("fifa")]
    public string FIFA { get; set; }
}

public class CountryNameDataDto
{
    [JsonPropertyName("common")]
    public string? Common { get; set; }

    [JsonPropertyName("official")]
    public string? Official { get; set; }

    [JsonPropertyName("nativeName")]
    public Dictionary<string, CountryTranslationDataDto> NativeName { get; set; }
}

public class CurrencyDetailDataDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }
}

public class CountryTranslationDataDto
{
    [JsonPropertyName("official")]
    public string Official { get; set; }

    [JsonPropertyName("common")]
    public string Common { get; set; }
}

public class DemonymDetailDataDto
{
    [JsonPropertyName("f")]
    public string Female { get; set; }

    [JsonPropertyName("m")]
    public string Male { get; set; }
}

public class CountryMapsDataDto
{
    [JsonPropertyName("googleMaps")]
    public string? GoogleMaps { get; set; }

    [JsonPropertyName("openStreetMaps")]
    public string? OpenStreetMaps { get; set; }
}

public class VehicleInfoDataDto
{
    [JsonPropertyName("signs")]
    public List<string> Signs { get; set; }

    [JsonPropertyName("side")]
    public string DrivingSide { get; set; }
}

public class CapitalCoordinatesDataDto
{
    [JsonPropertyName("latlng")]
    public List<double> LatLng { get; set; }
}

public class CountryFlagsDataDto
{
    [JsonPropertyName("png")]
    public string Png { get; set; }

    [JsonPropertyName("svg")]
    public string Svg { get; set; }

    [JsonPropertyName("alt")]
    public string Alt { get; set; }
}

public class CoatOfArmsDataDto
{
    [JsonPropertyName("png")]
    public string Png { get; set; }

    [JsonPropertyName("svg")]
    public string Svg { get; set; }
}

public class InternationalDialingCodeDataDto
{
    [JsonPropertyName("root")]
    public string Root { get; set; }

    [JsonPropertyName("suffixes")]
    public List<string> Suffixes { get; set; }
}