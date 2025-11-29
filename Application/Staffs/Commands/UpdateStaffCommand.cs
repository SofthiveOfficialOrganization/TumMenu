using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Staffs.Commands;

public sealed record UpdateStaffCommand
(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string? Role
) : IRequest<StaffDTO>, ITransactionalRequest;

public class UpdateStaffCommandHandler(
    IRepository<Staff> repoStaff,
    IMapper mapper
) : IRequestHandler<UpdateStaffCommand, StaffDTO>
{
    public async Task<StaffDTO> Handle(UpdateStaffCommand req, CancellationToken ct)
    {
        var staff = await repoStaff.GetByIdAsync(req.Id, ct);
        if(staff is null)
            throw new NotFoundAppException("Çalışan bulunamadı.");
        mapper.Map(req, staff);
        repoStaff.Update(staff);
        var staffDTO = mapper.Map<StaffDTO>(staff);
        return staffDTO;
    }
}
