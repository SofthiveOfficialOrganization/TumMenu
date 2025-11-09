using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Staffs.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs.Queries;
public sealed record GetStaffProfileByCurrentUserQuery : IRequest<StaffDTO>;

public sealed class GetStaffProfileByCurrentUserHandler(
	IRepository<Staff> repoStaff,
	IUserContext user,
	IMapper mapper
	) : IRequestHandler<GetStaffProfileByCurrentUserQuery, StaffDTO>
{
	public async Task<StaffDTO> Handle(GetStaffProfileByCurrentUserQuery request, CancellationToken cancellationToken)
	{
		if(!user.IsAuthenticated) throw new UnauthorizedAppException("Giriş gerekli.");

		if(!Guid.TryParse(user.StaffId, out var staffId))
			throw new ForbiddenAppException("Kullanıcının bilgilerinde çalışan yetkisi bulunamadı.");

		var staff = await repoStaff.Query()
			.Where(o => o.Id == staffId)
			.AsNoTracking()
			.FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundAppException("Çalışan profili bulunamadı.");

		var staffDTO = mapper.Map<StaffDTO?>(staff);

		return staffDTO!;
	}
}
