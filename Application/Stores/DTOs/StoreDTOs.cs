using Application.Common.Base.DTOs;
using Application.Companies.DTOs;
using Application.Staffs.DTOs;
using Domain.Entities;

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
	public string PhoneNumber { get; set; } = null!;
	public Guid CompanyId { get; set; }
	public CompanyDTO? Company { get; set; }
	public Address? Address { get; set; }
	public List<StaffDTO> Staffs { get; set; } = new List<StaffDTO>();
}
