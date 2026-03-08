using Application.QRs.Queries;
using Application.QRs.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class QRController(IMediator mediator, IServiceScopeFactory scopeFactory) : Controller
{
    [HttpGet("/q/{key}")]
    public async Task<IActionResult> RedirectToMenu(string key, [FromQuery] string? r, CancellationToken ct)
    {
        try
        {
            // 1. Resolve QR to find target URL and QR ID
            var result = await mediator.Send(new ResolveQRQuery(key), ct);

            // 2. Fire and Forget: Record the scan event in the background safely
            var qrCodeId = result.QRCodeId;
            var userAgent = Request.Headers.UserAgent.ToString();
            var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var referrer = r ?? Request.Headers.Referer.ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    // Create a new scope for the background task to avoid ObjectDisposedException
                    using var scope = scopeFactory.CreateScope();
                    var bgMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    await bgMediator.Send(new RecordQRScanCommand(
                        qrCodeId,
                        userAgent,
                        remoteIp,
                        referrer
                    ), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // In a real app, log this safely to a file or external service
                    System.Diagnostics.Debug.WriteLine($"Error in background QR scan recording: {ex.Message}");
                }
            });

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
