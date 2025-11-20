using Application.Abstractions; // IUnitOfWork
using Application.Auths.DTOs;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Auths.Commands;

public sealed record RegisterOwnerCommand(string Email, string UserName, string Password, string FirstName, string LastName) : IRequest<AuthResultDTO>;

public sealed class RegisterOwnerValidator : AbstractValidator<RegisterOwnerCommand>
{
	public RegisterOwnerValidator()
	{
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
		RuleFor(x => x.FirstName).NotEmpty();
		RuleFor(x => x.LastName).NotEmpty();
	}
}

public sealed class RegisterOwnerHandler(
	UserManager<ApplicationUser> users,
	RoleManager<ApplicationRole> roles,
	IJwtTokenService tokens,
	IUnitOfWork uow,
	IRepository<Owner> repoOwner,
	IMapper mapper
) : IRequestHandler<RegisterOwnerCommand, AuthResultDTO>
{
	public async Task<AuthResultDTO> Handle(RegisterOwnerCommand req, CancellationToken ct)
	{
		ApplicationUser? user = mapper.Map<ApplicationUser>(req);
		user.CreatedOn = DateTimeHelper.UtcNowSeconds();

		var create = await users.CreateAsync(user, req.Password);
		if(!create.Succeeded) throw new Exception(string.Join("; ", create.Errors.Select(e => e.Description)));

		if(!await roles.RoleExistsAsync("Owner"))
			await roles.CreateAsync(new ApplicationRole("Owner"));
		await users.AddToRoleAsync(user, "Owner");

		var owner = new Owner { ApplicationUserId = user.Id };
		await repoOwner.AddAsync(owner, ct);
		await uow.SaveChangesAsync(ct);

		var roleNames = await users.GetRolesAsync(user);
		var pair = await tokens.IssueAsync(user, roleNames, ct);
		return new AuthResultDTO(pair.AccessToken, pair.ExpiresAt, pair.RefreshToken);
	}
}
