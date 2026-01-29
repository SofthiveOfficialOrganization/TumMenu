using Application.Abstractions;
using Application.Common.Helpers;
using Application.Staffs.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Queries;

public class GetStaffByIdQuery : IRequest<StaffDTO>
{
	public Guid Id { get; set; }
}

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
		var staff = (await repoStaff.Query()
			.Include(s => s.Store)
			.FirstAsync(s => s.Id == req.Id, ct)).EnsureFound("Mağaza bulunamadı");
		var staffDTO = mapper.Map<StaffDTO>(staff);
		return staffDTO;
	}
}