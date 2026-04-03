using WebAPI.DataAccess.Paging;

namespace WebAPI.Models.Dtos.Province;

public class ProvinceListModel : BasePageableModel
{
    public IList<ProvinceDto>? Items { get; set; }
}