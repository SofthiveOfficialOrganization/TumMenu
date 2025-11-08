using Application.Abstractions;
using Application.Auth.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth;

public sealed record LoginCommand(LoginRequest Body) : IRequest<AuthResultDto>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
	public LoginValidator()
	{
		RuleFor(x => x.Body.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Body.Password).NotEmpty();
	}
}

public sealed class LoginHandler(
	SignInManager<ApplicationUser> signIn,
	UserManager<ApplicationUser> users,
	IJwtTokenService tokens
) : IRequestHandler<LoginCommand, AuthResultDto>
{
	public async Task<AuthResultDto> Handle(LoginCommand req, CancellationToken ct)
	{
		var user = await users.FindByEmailAsync(req.Body.Email)
				   ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

		var check = await signIn.CheckPasswordSignInAsync(user, req.Body.Password, lockoutOnFailure: true);
		if(!check.Succeeded) throw new UnauthorizedAccessException("Geçersiz kimlik bilgileri.");

		var roles = await users.GetRolesAsync(user);
		var pair = await tokens.IssueAsync(user, roles, ownerId: null, ct);
		return new AuthResultDto(pair.AccessToken, pair.ExpiresAt, pair.RefreshToken);
	}
}
