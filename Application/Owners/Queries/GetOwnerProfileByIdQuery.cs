using Application.Abstractions;
using Application.Owners.DTOs;
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

namespace Application.Owners.Queries;

public sealed record GetOwnerProfileByIdQuery(Guid Id) : IRequest<OwnerDTO>;

public sealed class GetOwnerProfileByIdQueryValidator : AbstractValidator<GetOwnerProfileByIdQuery>
{
	public GetOwnerProfileByIdQueryValidator()
	{
		RuleFor(x => x.Id).NotEmpty();
	}
}

public sealed class GetOwnerProfileByIdHandler(
	IRepository<Owner> repoOwner,
	IMapper mapper
) : IRequestHandler<GetOwnerProfileByIdQuery, OwnerDTO>
{
	public async Task<OwnerDTO> Handle(GetOwnerProfileByIdQuery req, CancellationToken ct)
	{
		var owner = await repoOwner.Query().FirstOrDefaultAsync(o => o.Id == req.Id, ct);
		var ownerDTO = mapper.Map<OwnerDTO>(owner!);
		return ownerDTO;
	}
}