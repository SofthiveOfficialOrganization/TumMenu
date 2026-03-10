using Application.Companies.Queries;
using Application.Medias.Commands;
using Application.Medias.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
public sealed class MediaController(IMediator mediator) : Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> Index(GetAllMediasPagedQuery req, CancellationToken ct)
    {
        var result = await mediator.Send(req, ct);
        ViewData["Title"] = "Tüm İçerikler";
        return View(result);
    }

    [Authorize(Policy = "OwnerOnly")]
    [HttpGet("MyMedias")]
    public async Task<IActionResult> MyMedias(GetAllMediasPagedQuery req, CancellationToken ct)
    {
        var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
        if (company == null) return Forbid();

        req.CompanyId = company.Id;
        var result = await mediator.Send(req, ct);
        ViewData["Title"] = "İçeriklerim";
        return View("Index", result);
    }

    [Authorize(Policy = "OwnerOnly")]
    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
        if (company == null) return Forbid();

        var result = await mediator.Send(new UploadMediaCommand(
            File: file,
            ReferenceId: company.Id,
            Type: MediaRefType.Company,
            Slot: "general-library"
        ), ct);

        return Ok(result);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMediaCommand(id), ct);
        return Ok();
    }
}
