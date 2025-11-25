using Application.Auths.DTOs;
using Application.Auths.Queries;
using Application.Owners.Queries;
using Application.Staffs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
[Route("staff")]
public class StaffController : ControllerBase
{
	[HttpGet("me")]
	[Authorize(Policy = "StaffOnly")]
	public async Task<IActionResult> Me([FromServices] IMediator mediator, CancellationToken ct)
		=> Ok(await mediator.Send(new GetStaffProfileByCurrentUserQuery(), ct));
}
