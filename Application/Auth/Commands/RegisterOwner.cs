using Application.Abstractions; // IUnitOfWork
using Application.Auth.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Commands;

public sealed record RegisterOwnerCommand(RegisterOwnerRequest Body) : IRequest<AuthResultDto>;

public sealed class RegisterOwnerValidator : AbstractValidator<RegisterOwnerCommand>
{
	public RegisterOwnerValidator()
	{
		RuleFor(x => x.Body.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Body.Password).NotEmpty().MinimumLength(6);
		RuleFor(x => x.Body.FirstName).NotEmpty();
		RuleFor(x => x.Body.LastName).NotEmpty();
	}
}

public sealed class RegisterOwnerHandler(
	UserManager<ApplicationUser> users,
	RoleManager<ApplicationRole> roles,
	IJwtTokenService tokens,
	IUnitOfWork uow,
	IRepository<Owner> repository,
	IMapper mapper
) : IRequestHandler<RegisterOwnerCommand, AuthResultDto>
{
	public async Task<AuthResultDto> Handle(RegisterOwnerCommand req, CancellationToken ct)
	{
		var body = req.Body;
		ApplicationUser? user = mapper.Map<ApplicationUser>(body);
		user.CreatedOn = DateTimeHelper.UtcNowSeconds();

		var create = await users.CreateAsync(user, body.Password);
		if(!create.Succeeded) throw new Exception(string.Join("; ", create.Errors.Select(e => e.Description)));

		if(!await roles.RoleExistsAsync("Owner"))
			await roles.CreateAsync(new ApplicationRole("Owner"));
		await users.AddToRoleAsync(user, "Owner");

		var owner = new Owner { ApplicationUserId = user.Id };
		await repository.AddAsync(owner, ct);
		await uow.SaveChangesAsync(ct);

		var roleNames = await users.GetRolesAsync(user);
		var pair = await tokens.IssueAsync(user, roleNames, ownerId: owner.Id.ToString(), ct);
		return new AuthResultDto(pair.AccessToken, pair.ExpiresAt, pair.RefreshToken);
	}
}
