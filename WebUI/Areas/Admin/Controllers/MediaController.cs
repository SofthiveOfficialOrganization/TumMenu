using Application.Companies.Queries;
using Application.Medias.Commands;
using Application.Medias.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

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
        if (file == null || file.Length == 0)
            return BadRequest("Dosya boş.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest("Geçersiz dosya türü. İzin verilen: jpg, jpeg, png, gif, webp.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
            return BadRequest("Dosya boyutu 5MB'ı geçemez.");

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




