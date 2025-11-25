using Application.Abstractions;
using Application.Staffs.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Queries;

public record GetStaffByIdQuery(Guid Id) : IRequest<StaffDTO>;

public class GetStaffProfileByIdValidator : AbstractValidator<GetStaffByIdQuery>
{
	public GetStaffProfileByIdValidator()
	{
		RuleFor(x => x.Id).NotEmpty();
	}
}
public sealed class GetStaffProfileByIdHandler(
	IRepository<Staff> repoStaff,
	IMapper mapper
) : IRequestHandler<GetStaffByIdQuery, StaffDTO>
{
	public async Task<StaffDTO> Handle(GetStaffByIdQuery req, CancellationToken ct)
	{
		var staff = await repoStaff.Query()
			.Include(s => s.Store)
			.FirstAsync(s => s.Id == req.Id, ct);
		var staffDTO = mapper.Map<StaffDTO>(staff);
		return staffDTO;
	}
}