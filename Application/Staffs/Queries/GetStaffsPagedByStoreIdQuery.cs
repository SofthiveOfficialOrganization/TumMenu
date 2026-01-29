using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Staffs.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System.Linq.Expressions;

namespace Application.Staffs.Queries;

public class GetStaffsPagedByStoreIdQuery : PageRequest, IRequest<PaginatedListDTO<StaffDTO>>
{
	public Guid StoreId { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
}

public class GetStaffsPagedByStoreIdHandler(
	IRepository<Staff> repoStaff,
	IMapper mapper
) : IRequestHandler<GetStaffsPagedByStoreIdQuery, PaginatedListDTO<StaffDTO>>
{
	public async Task<PaginatedListDTO<StaffDTO>> Handle(GetStaffsPagedByStoreIdQuery req, CancellationToken ct)
	{
		Expression<Func<Staff, bool>> filter = s =>
			s.StoreId == req.StoreId &&
			(string.IsNullOrWhiteSpace(req.FirstName) || s.FirstName.Contains(req.FirstName)) &&
			(string.IsNullOrWhiteSpace(req.LastName) || s.LastName.Contains(req.LastName)) &&
			(string.IsNullOrWhiteSpace(req.PhoneNumber) || (s.PhoneNumber != null && s.PhoneNumber.Contains(req.PhoneNumber))) &&
			(string.IsNullOrWhiteSpace(req.Email) || (s.Email != null && s.Email.Contains(req.Email)));

		var staffs = await repoStaff.GetPageListAsync(
			request: req,
			expression: filter,
			orderBy: q => q.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
			ct: ct
		);
		var staffDTOs = mapper.Map<PaginatedListDTO<StaffDTO>>(staffs);
		return staffDTOs;
	}
}