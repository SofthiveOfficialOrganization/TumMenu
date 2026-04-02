using Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class DashboardController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Kontrol Paneli";

        var dashboard = await mediator.Send(new GetOwnerDashboardQuery());
        var initialChartData = await mediator.Send(new GetQRChartDataQuery("weekly"));

        ViewData["InitialChartData"] = System.Text.Json.JsonSerializer.Serialize(
            initialChartData,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> QRChartData(string period = "weekly")
    {
        if (period != "weekly" && period != "monthly")
            period = "weekly";

        var data = await mediator.Send(new GetQRChartDataQuery(period));
        return Json(data);
    }
}
