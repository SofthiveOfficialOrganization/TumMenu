using Application.Common.Base.DTOs;
using Application.Staffs.DTOs;

namespace Application.Stores.DTOs;

public sealed class StoreLiteDTO : BaseDTO, ISluggableDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
}

public sealed class StoreDTO : BaseDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public List<StaffDTO> Staffs { get; set; } = new List<StaffDTO>();
}
