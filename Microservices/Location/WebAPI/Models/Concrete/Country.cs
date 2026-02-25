using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

/// <summary>
/// Ülke bilgilerini içeren temel model
/// </summary>
public class Country : BaseEntity<Guid>
{
    /// <summary>
    /// Ülkenin yaygın kullanılan adı (örneğin: "Türkiye")
    /// </summary>
    public string CommonName { get; set; }

    /// <summary>
    /// Ülkenin resmi adı (örneğin: "Türkiye Cumhuriyeti")
    /// </summary>
    public string OfficialName { get; set; }

    /// <summary>
    /// Ülkenin 2 harfli ISO kodu (örneğin: "TR")
    /// </summary>
    public string Alpha2Code { get; set; }

    /// <summary>
    /// Ülkenin 3 harfli ISO kodu (örneğin: "TUR")
    /// </summary>
    public string Alpha3Code { get; set; }

    /// <summary>
    /// Ülkenin 3 basamaklı numerik ISO kodu (örneğin: "792")
    /// </summary>
    public string? NumericCode { get; set; }

    /// <summary>
    /// Ülkenin bağımsız bir ülke olup olmadığını belirtir (true = bağımsız, false = bağımlı)
    /// </summary>
    public bool IsIndependent { get; set; }

    /// <summary>
    /// Ülkenin resmi statüsü (örneğin: "officially-assigned")
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Ülkenin Birleşmiş Milletler (UN) üyesi olup olmadığını belirtir
    /// </summary>
    public bool IsUnMember { get; set; }

    /// <summary>
    /// Ülkenin ait olduğu kıta veya bölge (örneğin: "Europe", "Americas")
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Ülkenin alt bölgesi (örneğin: "Western Europe", "Caribbean")
    /// </summary>
    public string? Subregion { get; set; }

    /// <summary>
    /// Ülkenin enlem (latitude) koordinatı
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Ülkenin boylam (longitude) koordinatı
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Ülkenin toplam yüzölçümü (km² cinsinden)
    /// </summary>
    public double? Area { get; set; }

    /// <summary>
    /// Ülkenin toplam nüfusu
    /// </summary>
    public long? Population { get; set; }

    /// <summary>
    /// Ülkenin bayrak emojisi (örneğin: "🇹🇷")
    /// </summary>
    public string? FlagEmoji { get; set; }

    /// <summary>
    /// Google Maps üzerinde ülkenin harita bağlantısı
    /// </summary>
    public string? GoogleMaps { get; set; }

    /// <summary>
    /// OpenStreetMap üzerinde ülkenin harita bağlantısı
    /// </summary>
    public string? OpenStreetMap { get; set; }

    /// <summary>
    /// Ülkenin zaman dilimi bilgisi (örneğin: "UTC+03:00")
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Ülkede haftanın başlangıç günü (örneğin: "monday")
    /// </summary>
    public string? StartOfWeek { get; set; }

    /// <summary>
    /// Ülkenin sahip olduğu para birimleri
    /// </summary>
    public virtual ICollection<CountryCurrency>? Currencies { get; set; }

    /// <summary>
    /// Ülkede konuşulan diller
    /// </summary>
    public virtual ICollection<CountryLanguage>? Languages { get; set; }

    /// <summary>
    /// Ülkenin farklı dillerdeki çevirileri
    /// </summary>
    public virtual ICollection<CountryTranslation>? Translations { get; set; }

    public Country()
    {
        Currencies = new HashSet<CountryCurrency>();
        Languages = new HashSet<CountryLanguage>();
        Translations = new HashSet<CountryTranslation>();
    }

    /// <summary>
    /// Ülke bilgilerini güncelleyen metot
    /// </summary>
    public void Update(
        string commonName, string officialName, string alpha2Code, string alpha3Code, string? numericCode,
        bool isIndependent, string status, bool isUnMember, string region, string? subregion, double? latitude, 
        double? longitude, double? area, long? population, string? flagEmoji, string? googleMaps, 
        string? openStreetMap, string? timezone, string? startOfWeek)
    {
        CommonName = commonName;
        OfficialName = officialName;
        Alpha2Code = alpha2Code;
        Alpha3Code = alpha3Code;
        NumericCode = numericCode;
        IsIndependent = isIndependent;
        Status = status;
        IsUnMember = isUnMember;
        Region = region;
        Subregion = subregion;
        Latitude = latitude;
        Longitude = longitude;
        Area = area;
        Population = population;
        FlagEmoji = flagEmoji;
        GoogleMaps = googleMaps;
        OpenStreetMap = openStreetMap;
        Timezone = timezone;
        StartOfWeek = startOfWeek;
        UpdatedDate = DateTime.UtcNow;
    }
}
