using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

/// <summary>
/// Ülkede konuşulan dilleri temsil eden model
/// </summary>
public class CountryLanguage : BaseEntity<Guid>
{
    /// <summary>
    /// Dilin ait olduğu ülke kimliği
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// Dilin ISO kodu (örneğin: "eng", "tur")
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Dilin adı (örneğin: "English", "Türkçe")
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// İlgili ülke ile bağlantı
    /// </summary>
    public virtual Country? Country { get; set; }

    public void Update(string code, string name)
    {
        Code = code;
        Name = name;
        UpdatedDate = DateTime.UtcNow;
    }
}