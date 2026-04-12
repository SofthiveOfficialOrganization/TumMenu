using Application.Companies.Queries;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebUI.Filters;

public class EnsureCompanyExistsFilter : IAsyncActionFilter
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public EnsureCompanyExistsFilter(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // 0. Exclude public Home/Index from this filter to allow logged-in users to see the landing page
        var currentController = context.RouteData.Values["controller"]?.ToString();
        var currentAction = context.RouteData.Values["action"]?.ToString();
        var currentArea = context.RouteData.Values["area"]?.ToString();

        if (string.IsNullOrEmpty(currentArea) && 
            string.Equals(currentController, "Home", StringComparison.OrdinalIgnoreCase) && 
            string.Equals(currentAction, "Index", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        // 1. Check if user is authenticated and is an Owner
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Owner"))
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                await context.HttpContext.SignOutAsync();
                context.Result = new RedirectResult("/giris");
                return;
            }

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

            // If the owner record no longer exists (for example the account was deleted by an admin),
            // the auth cookie can still carry stale "Owner" claims until it is revalidated.
            var ownerExists = await _db.Owners
                .AsNoTracking()
                .AnyAsync(o => o.ApplicationUserId == userId, context.HttpContext.RequestAborted);

            if (!ownerExists)
            {
                await context.HttpContext.SignOutAsync();
                context.Result = new RedirectResult("/giris");
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
