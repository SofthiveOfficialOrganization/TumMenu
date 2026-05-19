namespace WebUI.Seo;

/// <summary>Yerel SEO için şehir slug → görünen ad eşlemesi (dinamik mağaza URL'lerinden bağımsız).</summary>
public static class CitySeoCatalog
{
    public sealed record CitySeoEntry(string Slug, string DisplayName);

    private static readonly IReadOnlyDictionary<string, CitySeoEntry> Cities =
        new Dictionary<string, CitySeoEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["istanbul"] = new("istanbul", "İstanbul"),
            ["ankara"] = new("ankara", "Ankara"),
            ["izmir"] = new("izmir", "İzmir"),
            ["bursa"] = new("bursa", "Bursa"),
            ["antalya"] = new("antalya", "Antalya"),
            ["adana"] = new("adana", "Adana"),
            ["konya"] = new("konya", "Konya"),
            ["gaziantep"] = new("gaziantep", "Gaziantep"),
            ["sanliurfa"] = new("sanliurfa", "Şanlıurfa"),
            ["mersin"] = new("mersin", "Mersin")
        };

    public static CitySeoEntry? TryGet(string? slug) =>
        slug is not null && Cities.TryGetValue(slug, out var entry) ? entry : null;

    public static IReadOnlyList<CitySeoEntry> All => Cities.Values.OrderBy(c => c.DisplayName).ToList();
}
