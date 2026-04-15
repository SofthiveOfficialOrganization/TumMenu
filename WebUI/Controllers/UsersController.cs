using Application.Auths.DTOs;
using Application.Auths.Queries;
using Application.Owners.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[ApiController]
[Route("user")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<SessionDTO>> Me(CancellationToken ct)
        => Ok(await mediator.Send(new GetSessionByCurrentUserQuery(), ct));
}

[ApiController]
[Route("owner")]
public class OwnerController : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> Me([FromServices] IMediator mediator, CancellationToken ct)
        => Ok(await mediator.Send(new GetOwnerProfileByCurrentUserQuery(), ct));
}

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;

	public AuthController(
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginRequest request)
	{
		var result = await _signInManager.PasswordSignInAsync(
			request.Email,
			request.Password,
			false,
			lockoutOnFailure: false);

		if(!result.Succeeded)
			return Unauthorized("Email veya şifre hatalı");

		var user = await _userManager.FindByEmailAsync(request.Email);
		var roles = await _userManager.GetRolesAsync(user!);

		if(!roles.Contains("Admin") && !roles.Contains("Owner"))
		{
			await _signInManager.SignOutAsync();
			return Forbid();
		}

		return Ok("Login başarılı");
	}
}
public class LoginRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}