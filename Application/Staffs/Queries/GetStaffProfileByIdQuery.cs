using Application.Abstractions;
using Application.Staffs.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs.Queries;

public record GetStaffProfileByIdQuery(Guid Id) : IRequest<StaffDTO>;

public class GetStaffProfileByIdValidator : AbstractValidator<GetStaffProfileByIdQuery>
{
	public GetStaffProfileByIdValidator()
	{
		RuleFor(x => x.Id).NotEmpty();
	}
}
public sealed class GetStaffProfileByIdHandler(IRepository<Staff> repoStaff, IMapper mapper) : IRequestHandler<GetStaffProfileByIdQuery, StaffDTO>
{
	public async Task<StaffDTO> Handle(GetStaffProfileByIdQuery query, CancellationToken ct)
	{
		var staff = await repoStaff.Query().FirstAsync(s => s.Id == query.Id, ct);
		var staffDTO = mapper.Map<StaffDTO>(staff);
		return staffDTO;
	}
}