using WebAPI.DataAccess.Paging;

namespace WebAPI.Models.Dtos.District;

public class DistrictListModel : BasePageableModel
{
    public IList<DistrictDto> Items { get; set; }
}