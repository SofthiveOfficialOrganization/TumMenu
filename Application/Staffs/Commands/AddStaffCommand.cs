using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Commands;

public class AddStaffCommand : IRequest<StaffDTO>, ITransactionalRequest
{
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Role { get; set; }
	public Guid CompanyId { get; set; }
}

public class AddStaffCommandValidator : AbstractValidator<AddStaffCommand>
{
	public AddStaffCommandValidator()
	{
		RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
		RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
		RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Email));
		RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
		RuleFor(x => x.Role).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Role));
		RuleFor(x => x.CompanyId).NotEmpty();
	}
}

public class AddStaffCommandHandler(
	IRepository<Staff> repoStaff,
	IMapper mapper
) : IRequestHandler<AddStaffCommand, StaffDTO>
{
	public async Task<StaffDTO> Handle(AddStaffCommand req, CancellationToken ct)
	{
		var staff = await repoStaff.Query().AnyAsync(s => s.FirstName == req.FirstName && s.LastName == req.LastName, ct);
		var newStaff = mapper.Map<Staff>(req);
		await repoStaff.AddAsync(newStaff, ct);
		var staffDTO = mapper.Map<StaffDTO>(newStaff);
		return staffDTO;
	}
}
