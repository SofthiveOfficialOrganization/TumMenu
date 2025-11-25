using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Staffs.DTOs;
using Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs.Queries;

public record GetStaffsPagedByStoreIdQuery(
	Guid StoreId,
	string? FirstName,
	string? LastName,
	string? PhoneNumber,
	string? Email
) : PageRequest, IRequest<PaginatedListDTO<StaffDTO>>;

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