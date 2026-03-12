using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebUI.Filters;

public class EnsureCompanyExistsFilter : IAsyncActionFilter
{
    private readonly IMediator _mediator;

    public EnsureCompanyExistsFilter(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // 1. Check if user is authenticated and is an Owner
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Owner"))
        {
            // 1.1 exclude API requests from redirection
            if (context.HttpContext.Request.Path.Value?.StartsWith("/api", StringComparison.OrdinalIgnoreCase) == true)
            {
                await next();
                return;
            }

            // 2. exclude Identity area (Logout, Manage, etc.) 
            // We want to allow them to logout or manage account if they want
            var area = context.RouteData.Values["area"]?.ToString();
            if (string.Equals(area, "Identity", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 3. Check if already on Onboarding controller
            var controller = context.RouteData.Values["controller"]?.ToString();
            if (string.Equals(controller, "Onboarding", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 3.1 Check if action is "CheckSlug" (used in onboarding validation)
            var action = context.RouteData.Values["action"]?.ToString();
            if (string.Equals(action, "CheckSlug", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
            
            // 4. Check if they have completed the Onboarding Wizard
            try
            {
                var isWizardCompleted = await _mediator.Send(new Application.Owners.Queries.GetWizardStatusQuery());
                if (!isWizardCompleted)
                {
                    context.Result = new RedirectToActionResult("Index", "Onboarding", new { area = "Admin" });
                    return;
                }
            }
            catch (Exception)
            {
                // Silently fallback if owner not found correctly here? Or force onboarding?
                // Safest to force onboarding if we can't determine status
                context.Result = new RedirectToActionResult("Index", "Onboarding", new { area = "Admin" });
                return;
            }
        }

        await next();
    }
}
