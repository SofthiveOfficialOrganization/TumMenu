using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

/// <summary>
/// Ülkenin sahip olduğu para birimi bilgilerini içeren model.
/// </summary>
public class CountryCurrency : BaseEntity<Guid>
{
    /// <summary>
    /// Para biriminin bağlı olduğu ülkenin kimliği.
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// Para biriminin ISO 4217 kodu (örneğin: "USD", "EUR").
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Para biriminin adı (örneğin: "United States Dollar").
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Para biriminin sembolü (örneğin: "$", "€").
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Para biriminin bağlı olduğu ülke.
    /// </summary>
    public virtual Country? Country { get; set; }

    public void Update(string code, string name, string? symbol = null)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        UpdatedDate = DateTime.UtcNow;
    }
}