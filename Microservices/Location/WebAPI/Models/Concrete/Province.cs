using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

public class Province : BaseEntity<Guid>
{
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? GoogleMaps { get; set; }
    public string? OpenStreetMap { get; set; }

    public virtual ICollection<District>? Districts { get; set; }

    public void Update(
        string name, double latitude, double longitude,
        string googleMaps, string? openStreetMap, DateTime deletedDate)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        GoogleMaps = googleMaps;
        OpenStreetMap = openStreetMap;
        DeletedDate = deletedDate;
        UpdatedDate = DateTime.UtcNow;
    }
}