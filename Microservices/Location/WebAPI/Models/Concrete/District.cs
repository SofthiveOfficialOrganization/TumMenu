using WebAPI.DataAccess.Repositories;

namespace WebAPI.Models.Concrete;

public class District : BaseEntity<Guid>
{
    public Guid ProvinceId { get; set; }
    public string Name { get; set; }
    public int Population { get; set; }
    public int Area { get; set; }

    public virtual Province Province { get; set; }
    
    public void Update(
        Guid provinceId, string name, int population, 
        int area, DateTime deletedDate)
    {
        ProvinceId = provinceId;
        Name = name;
        Population = population;
        Area = area;
        DeletedDate = deletedDate;
        UpdatedDate = DateTime.UtcNow;
    }
}