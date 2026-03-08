using Application.Ads.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebUI.ViewComponents;

public class AdViewComponent(IMediator mediator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string slotKey)
    {
        var activeAd = await mediator.Send(new GetActiveAdBySlotKeyQuery(slotKey));
        return View(activeAd);
    }
}
