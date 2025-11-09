using Application.Auths;
using Application.Auths.Commands;
using Application.Auths.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TumMenu.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
	[HttpPost("register-owner")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthResultDTO>> Register([FromBody] RegisterOwnerCommand request, CancellationToken ct)
		=> Ok(await mediator.Send(request, ct));

	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthResultDTO>> Login([FromBody] LoginCommand request, CancellationToken ct)
		=> Ok(await mediator.Send(request, ct));


	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<ActionResult<AuthResultDTO>> Refresh([FromBody] RefreshTokenCommand request, CancellationToken ct)
		=> Ok(await mediator.Send(request, ct));

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout(CancellationToken ct)
		=> Ok(await mediator.Send(new LogoutCommand(), ct));
}
