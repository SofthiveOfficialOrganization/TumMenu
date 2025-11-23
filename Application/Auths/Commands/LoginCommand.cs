using Application.Abstractions;
using Application.Auths.DTOs;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auths.Commands;

public sealed record LoginCommand(string EmailOrUserName, string Password) : IRequest<AuthResultDTO>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    IJwtTokenService tokens
) : IRequestHandler<LoginCommand, AuthResultDTO>
{
    public async Task<AuthResultDTO> Handle(LoginCommand req, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(req.EmailOrUserName)
                   ?? await users.FindByNameAsync(req.EmailOrUserName)
                   ?? throw new UnauthorizedAccessException("Geçersiz kimlik bilgileri.");

        var check = await signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if(!check.Succeeded) throw new UnauthorizedAccessException("Geçersiz kimlik bilgileri.");

        var roles = await users.GetRolesAsync(user);
        var pair = await tokens.IssueAsync(user, roles, ct);
        return new AuthResultDTO(pair.AccessToken, pair.ExpiresAt, pair.RefreshToken);
    }
}
