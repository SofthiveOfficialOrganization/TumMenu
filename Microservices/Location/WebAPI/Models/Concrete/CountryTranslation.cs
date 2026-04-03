using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

/// <summary>
/// Ülkenin farklı dillerdeki isim çevirilerini temsil eden model
/// </summary>
public class CountryTranslation : BaseEntity<Guid>
{
    /// <summary>
    /// Çevirinin ait olduğu ülke kimliği
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// Çeviri yapılan dilin ISO kodu (örneğin: "ara", "fra", "spa")
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Ülkenin resmi adı bu dilde (örneğin: "République de Turquie" - Fransızca için)
    /// </summary>
    public string? OfficialName { get; set; }

    /// <summary>
    /// Ülkenin yaygın adı bu dilde (örneğin: "Turquie" - Fransızca için)
    /// </summary>
    public string? CommonName { get; set; }


    /// <summary>
    /// İlgili ülke ile bağlantı
    /// </summary>
    public virtual Country? Country { get; set; }

    public void Update(string languageCode, string? commonName, string? officialName = null)
    {
        LanguageCode = languageCode;
        OfficialName = officialName;
        CommonName = commonName;
        UpdatedDate = DateTime.UtcNow;
    }
}