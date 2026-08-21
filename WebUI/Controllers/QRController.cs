using Application.QRs.Queries;
using Application.QRs.Commands;
using Application.CustomerOrderRequests;
using Application.CustomerOrderRequests.Commands;
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
                    
                    await bgMediator.Send(new RecordQRScanCommand
                    {
                        QRCodeId = qrCodeId,
                        UserAgent = userAgent,
                        IpAddress = remoteIp,
                        Referrer = referrer
                    }, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // In a real app, log this safely to a file or external service
                    System.Diagnostics.Debug.WriteLine($"Error in background QR scan recording: {ex.Message}");
                }
            });

            try
            {
                var orderSession = await mediator.Send(new CreateQrOrderSessionCommand(
                    result.QRCodeId,
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()), ct);

                Response.Cookies.Append(
                    OrderSessionToken.CookieName,
                    orderSession.Token,
                    new CookieOptions
                    {
                        Expires = orderSession.ExpiresAt,
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = Request.IsHttps
                    });
            }
            catch (Application.Common.Exceptions.NotFoundAppException)
            {
                // Store-bound QR codes can create order sessions; static QR targets still redirect normally.
            }

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
