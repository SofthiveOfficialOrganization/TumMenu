using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Staffs.Commands;

public class UpdateStaffCommand : IRequest<StaffDTO>, ITransactionalRequest, IEntityAuditableCommand
{
	public string ActionName => "Personel güncellendi";
	public Guid EntityId => Id;
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}

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
