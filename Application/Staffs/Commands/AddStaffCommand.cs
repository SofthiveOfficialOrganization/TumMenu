using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Commands;

public sealed record AddStaffCommand
(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Role,
    Guid CompanyId
) : IRequest<StaffDTO>, ITransactionalRequest;

public class AddStaffValidator : AbstractValidator<AddStaffCommand>
{
    public AddStaffValidator()
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
        if(staff)
            throw new AlreadyExistsAppException("Girilen isim ve soyisime ait bir çalışan bulunmaktadır.");
        var newStaff = mapper.Map<Staff>(req);
        await repoStaff.AddAsync(newStaff, ct);
        var staffDTO = mapper.Map<StaffDTO>(newStaff);
        return staffDTO;
    }
}

