using Application.QRs.Queries;
using Application.QRs.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class QRController(IMediator mediator) : Controller
{
    [HttpGet("/q/{key}")]
    public async Task<IActionResult> RedirectToMenu(string key, [FromQuery] string? r, CancellationToken ct)
    {
        try
        {
            // 1. Resolve QR to find target URL and QR ID
            var result = await mediator.Send(new ResolveQRQuery(key), ct);

            // 2. Fire and Forget: Record the scan event in the background
            // We don't wait for this to finish to keep the redirect fast
            _ = Task.Run(async () =>
            {
                try
                {
                    await mediator.Send(new RecordQRScanCommand(
                        result.QRCodeId,
                        Request.Headers.UserAgent.ToString(),
                        Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        r ?? Request.Headers.Referer.ToString()
                    ), CancellationToken.None);
                }
                catch
                {
                    // Log error if needed, but don't crash the request
                }
            }, CancellationToken.None);

            // 3. Redirect to the target menu
            return Redirect(result.Url);
        }
        catch (Application.Common.Exceptions.NotFoundAppException ex)
        {
            return NotFound(ex.Message);
        }
        catch
        {
            return Redirect("/");
        }
    }
}
