using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Owners.DTOs;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Owners.Queries;
public sealed record GetOwnerProfileByCurrentUserQuery() : IRequest<OwnerDTO>;

public sealed class GetOwnerProfileByCurrentUserHandler(
	IRepository<Owner> repoOwner,
	IUserContext user,
	IMapper mapper
) : IRequestHandler<GetOwnerProfileByCurrentUserQuery, OwnerDTO>
{
	public async Task<OwnerDTO> Handle(GetOwnerProfileByCurrentUserQuery req, CancellationToken ct)
	{
		if(!user.IsAuthenticated) throw new UnauthorizedAppException("Giriş gerekli.");

		if(!Guid.TryParse(user.OwnerId, out var ownerId))
			throw new ForbiddenAppException("Owner yetkisi bulunamadı.");

		var owner = await repoOwner.Query()
			.Where(o => o.Id == ownerId)
			.AsNoTracking()
			.FirstOrDefaultAsync(ct) ?? throw new NotFoundAppException("Owner profili bulunamadı.");

		var ownerDTO = mapper.Map<OwnerDTO?>(owner);

		return ownerDTO!;
	}
}