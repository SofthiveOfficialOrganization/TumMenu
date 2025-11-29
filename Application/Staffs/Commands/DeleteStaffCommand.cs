using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;

namespace Application.Staffs.Commands;

public sealed record DeleteStaffCommand
(
	Guid Id
) : IRequest<Unit>, ITransactionalRequest;

public class DeleteStaffCommandHandler(
	IRepository<Staff> repoStaff
) : IRequestHandler<DeleteStaffCommand, Unit>
{
	public async Task<Unit> Handle(DeleteStaffCommand req, CancellationToken ct)
	{
		var staff = await repoStaff.GetByIdAsync(req.Id, ct);
		if(staff is null)
			throw new NotFoundAppException("Kullanıcı bulunamadı");
		repoStaff.SoftDelete(staff);
		return Unit.Value;
	}
}