using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using Domain.Helpers;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs.Queries;

public record GetAllStaffsPagedQuery(
	string? FirstName,
	string? LastName,
	string? PhoneNumber,
	string? Email
) : PageRequest, IRequest<PaginatedListDTO<StaffDTO>>;

public class GetAllStaffsPagedHandler(
	IRepository<Staff> repoStaff,
	IMapper mapper
) : IRequestHandler<GetAllStaffsPagedQuery, PaginatedListDTO<StaffDTO>>
{
	public async Task<PaginatedListDTO<StaffDTO>> Handle(GetAllStaffsPagedQuery req, CancellationToken ct)
	{
		Expression<Func<Staff, bool>> filter = s =>
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

		if(staffs.Items.Count == 0)
			throw new NotFoundAppException("Hiç personel bulunamadı.");

		var staffDTOs = mapper.Map<PaginatedListDTO<StaffDTO>>(staffs);
		return staffDTOs;
	}

}
