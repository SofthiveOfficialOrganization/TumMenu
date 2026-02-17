using Application.Companies.Commands;
using Application.Companies.DTOs;
using Application.Menus.Commands;
using Application.Menus.DTOs;
using Application.Stores.Commands;
using Application.Stores.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken] // Debugging: Disable CSRF to rule it out for 400 Bad Request
public sealed class OnboardingController(IMediator mediator, ILogger<OnboardingController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("company")]
    public async Task<ActionResult<CompanyDTO>> CreateCompany([FromBody] CreateCompanyCommand command, CancellationToken ct)
    {
        try 
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { Message = "Validation Failed", Errors = errors });
            }
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating company");
            throw; // Let filter handle it, but logged first
        }
    }

    [HttpPost("store")]
    public async Task<ActionResult<StoreDTO>> CreateStore([FromBody] CreateStoreCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("menu")]
    public async Task<ActionResult<MenuDTO>> CreateMenu([FromBody] CreateMenuToStoreCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }
}
