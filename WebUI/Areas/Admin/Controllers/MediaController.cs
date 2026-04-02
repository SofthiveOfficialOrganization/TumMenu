using Application.Companies.Queries;
using Application.Medias.Commands;
using Application.Medias.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class MediaController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Index(GetAllMediasPagedQuery req, CancellationToken ct)
    {
        if (User.IsInRole("Admin"))
        {
            var result = await mediator.Send(req, ct);
            ViewData["Title"] = "Tüm İçerikler";
            return View(result);
        }
        else
        {
            var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
            if (company == null) return Forbid();

            req.CompanyId = company.Id;
            var result = await mediator.Send(req, ct);
            ViewData["Title"] = "İçeriklerim";
            return View(result);
        }
    }



    [Authorize(Policy = "OwnerOnly")]
    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
        if (company == null) return Forbid();

        var result = await mediator.Send(new UploadMediaCommand
        {
            File = file,
            ReferenceId = company.Id,
            Type = MediaRefType.Company,
            Slot = "general-library"
        }, ct);

        return Ok(result);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMediaCommand { Id = id }, ct);
        return Ok();
    }
}




