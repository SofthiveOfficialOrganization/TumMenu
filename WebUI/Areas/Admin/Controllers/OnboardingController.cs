using Application.Companies.Commands;
using Application.Companies.DTOs;
using Application.Menus.Commands;
using Application.Menus.DTOs;
using Application.Stores.Commands;
using Application.Stores.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[IgnoreAntiforgeryToken] // Debugging: Disable CSRF to rule it out for 400 Bad Request
[Route("admin/Onboarding")]
public sealed class OnboardingController(IMediator mediator, ILogger<OnboardingController> logger) : Controller
{
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Check state to resume wizard
        var company = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
        
        if (company != null)
        {
            ViewBag.CompanyId = company.Id;
            ViewBag.CompanySlug = company.Slug;
            
            var stores = await mediator.Send(new Application.Stores.Queries.GetStoresPagedQuery 
            { 
                CompanyId = company.Id, 
                PageSize = 1 
            }, ct);

            if (stores.Items.Any())
            {
                var store = stores.Items.First();
                ViewBag.StoreId = store.Id;
                ViewBag.StoreSlug = store.Slug;

                // Check for menus on that store
                var menus = await mediator.Send(new Application.Menus.Queries.GetMenusPagedByStoreQuery
                {
                    StoreId = store.Id,
                    PageSize = 1
                }, ct);

                if (menus.Items.Any())
                {
                    ViewBag.MenuId = menus.Items.First().Id;
                    ViewBag.InitialStep = 3; // Go to Category creation
                }
                else
                {
                    ViewBag.InitialStep = 2; // Go to Menu creation
                }
            }
            else
            {
                ViewBag.InitialStep = 1; // Go to Store creation
            }
        }
        else
        {
            ViewBag.InitialStep = 0; // Go to Company creation
        }

        return View();
    }

    [HttpPost("company")]
    public async Task<ActionResult<CompanyDTO>> CreateCompany([FromBody] CreateCompanyCommand command, CancellationToken ct)
    {
        try 
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { Message = "Validation Failed", Errors = errors });
            }
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating company");
            throw; // Let filter handle it, but logged first
        }
    }

    [HttpPost("store")]
    public async Task<ActionResult<StoreDTO>> CreateStore([FromBody] CreateStoreCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("menu")]
    public async Task<ActionResult<MenuDTO>> CreateMenu([FromBody] CreateMenuToStoreCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Search the category library for items that can be added to this menu.
    /// Excludes items already linked to the menu.
    /// </summary>
    [HttpGet("category-library")]
    public async Task<IActionResult> SearchCategoryLibrary([FromQuery] string? search, [FromQuery] Guid menuId, CancellationToken ct)
    {
        // Fetch already-added category library item IDs for this menu to exclude them
        List<Guid> excludeIds = new();
        if (menuId != Guid.Empty)
        {
            var menu = await mediator.Send(new Application.Menus.Queries.GetMenuByIdQuery { Id = menuId }, ct);
            excludeIds = menu?.Categories?.Select(c => c.CategoryLibraryItemId).ToList() ?? [];
        }

        var result = await mediator.Send(new Application.Categories.Queries.GetAllCategoryLibraryItemsPagedQuery
        {
            Search = search,
            ExcludeIds = excludeIds,
            PageSize = 50
        }, ct);

        return Ok(result.Items.Select(x => new { x.Id, x.Title, x.Description }));
    }

    /// <summary>
    /// Get categories already linked to this menu (to pre-populate the right panel on step 4).
    /// </summary>
    [HttpGet("menu-categories")]
    public async Task<IActionResult> GetMenuCategories([FromQuery] Guid menuId, CancellationToken ct)
    {
        if (menuId == Guid.Empty) return Ok(Array.Empty<object>());

        var menu = await mediator.Send(new Application.Menus.Queries.GetMenuByIdQuery { Id = menuId }, ct);
        if (menu?.Categories == null) return Ok(Array.Empty<object>());

        var result = menu.Categories
            .OrderBy(c => c.SortOrder)
            .Select(c => new
            {
                id = c.Id,   // Category record Id (used for remove)
                title = c.CategoryLibraryItem?.Title ?? "(Başlıksız)"
            });

        return Ok(result);
    }

    /// <summary>
    /// Remove a category from the menu (by category record Id, not library item Id).
    /// </summary>
    [HttpPost("remove-category/{id:guid}")]
    public async Task<IActionResult> RemoveCategory(Guid id, CancellationToken ct)
    {
        await mediator.Send(new Application.Menus.Commands.RemoveCategoryFromMenuCommand { Id = id }, ct);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Add an existing CategoryLibraryItem to the menu.
    /// </summary>
    [HttpPost("add-category")]
    public async Task<IActionResult> AddCategory([FromBody] AddOnboardingCategoryRequest req, CancellationToken ct)
    {
        if (req.MenuId == Guid.Empty || req.CategoryLibraryItemId == Guid.Empty)
            return BadRequest("Geçersiz istek");

        var result = await mediator.Send(new Application.Menus.Commands.AddCategoryToMenuCommand
        {
            MenuId = req.MenuId,
            CategoryLibraryItemId = req.CategoryLibraryItemId,
            SortOrder = req.SortOrder,
            IsActive = true
        }, ct);

        return Ok(new { id = result.Id, title = result.CategoryLibraryItem?.Title });
    }

    /// <summary>
    /// Mark the wizard as completed (called when user clicks 'Finish' on the category step).
    /// </summary>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteOnboarding(CancellationToken ct)
    {
        await mediator.Send(new Application.Owners.Commands.MarkWizardCompletedCommand(), ct);
        return Ok(new { success = true });
    }
}

public class AddOnboardingCategoryRequest
{
    public Guid MenuId { get; set; }
    public Guid CategoryLibraryItemId { get; set; }
    public int SortOrder { get; set; }
}

