using Application.Ads.Queries;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebUI.ViewComponents;

public class AdViewComponent(IMediator mediator, IConfiguration configuration) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string slotKey)
    {
        var enableAds = configuration.GetValue<bool>("Ads:EnableAdSense", false);
        if (!enableAds)
        {
            return Content(string.Empty);
        }

        var activeAd = await mediator.Send(new GetActiveAdBySlotKeyQuery(slotKey));
        return View(activeAd);
    }
}
