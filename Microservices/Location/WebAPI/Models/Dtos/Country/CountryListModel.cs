using WebAPI.DataAccess.Paging;

namespace WebAPI.Models.Dtos.Country;

public class CountryListModel : BasePageableModel
{
    public IList<CountryDto> Items { get; set; }
}