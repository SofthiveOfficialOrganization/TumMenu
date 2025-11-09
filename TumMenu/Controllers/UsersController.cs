using Application.Auths.DTOs;
using Application.Auths.Queries;
using Application.Owners.Queries;
using Application.Staffs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TumMenu.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UserController(IMediator mediator) : ControllerBase
{
	[HttpGet("me")]
	[Authorize]
	public async Task<ActionResult<SessionDTO>> Me(CancellationToken ct)
		=> Ok(await mediator.Send(new GetSessionByCurrentUserQuery(), ct));
}

[ApiController]
[Route("api/owner")]
public class OwnerController : ControllerBase
{
	[HttpGet("me")]
	[Authorize(Policy = "OwnerOnly")]
	public async Task<IActionResult> Me([FromServices] IMediator mediator, CancellationToken ct)
		=> Ok(await mediator.Send(new GetOwnerProfileByCurrentUserQuery(), ct));
}

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
	[HttpGet("me")]
	[Authorize(Policy = "StaffOnly")]
	public async Task<IActionResult> Me([FromServices] IMediator mediator, CancellationToken ct)
		=> Ok(await mediator.Send(new GetStaffProfileByCurrentUserQuery(), ct));
}
