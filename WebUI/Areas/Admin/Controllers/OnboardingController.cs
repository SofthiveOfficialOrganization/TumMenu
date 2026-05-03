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
            ViewBag.IsSingleStore = company.IsSingleStore;
            
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

                // Check for company main menu
                var menus = await mediator.Send(new Application.Menus.Queries.GetMenusPagedByCompanyQuery
                {
                    CompanyId = company.Id,
                    PageSize = 1
                }, ct);

                if (menus.Items.Any())
                {
                    ViewBag.MenuId = menus.Items.First().Id;
                    ViewBag.InitialStep = 4; // Go to Category creation
                }
                else
                {
                    ViewBag.InitialStep = 3; // Go to Menu creation
                }
            }
            else
            {
                ViewBag.InitialStep = 2; // Go to Store creation
            }
        }
        else
        {
            ViewBag.InitialStep = 0; // Go to business type selection
        }

        return View();
    }

    [HttpPost("company")]
    [IgnoreAntiforgeryToken]
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
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<StoreDTO>> CreateStore([FromBody] CreateStoreCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("menu")]
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<MenuDTO>> CreateMenu([FromBody] OnboardingCreateMenuRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var companyMenu = await mediator.Send(new CreateMenuToCompanyCommand
        {
            Title = req.Title,
            CompanyId = req.CompanyId
        }, ct);

        // Also create an empty store menu so the store appears in "Dükkan Menülerim"
        if (req.StoreId.HasValue && req.StoreId.Value != Guid.Empty)
        {
            await mediator.Send(new CreateMenuToStoreCommand
            {
                Title = req.Title,
                StoreId = req.StoreId.Value,
                Status = Domain.Entities.MenuStatus.Active
            }, ct);
        }

        return Ok(companyMenu);
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
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RemoveCategory(Guid id, CancellationToken ct)
    {
        var category = await mediator.Send(new Application.Categories.Queries.GetCategoryByIdQuery(id), ct);
        await mediator.Send(new Application.Menus.Commands.RemoveCategoryFromMenuCommand { Id = id }, ct);

        if (Guid.TryParse(Request.Query["storeId"], out var storeId))
        {
            await SyncStoreMenuIfRequested(category.MenuId, storeId, ct);
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Add an existing CategoryLibraryItem to the menu.
    /// </summary>
    [HttpPost("add-category")]
    [IgnoreAntiforgeryToken]
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

        await SyncStoreMenuIfRequested(req.MenuId, req.StoreId, ct);

        return Ok(new { id = result.Id, title = result.CategoryLibraryItem?.Title });
    }

    [HttpPost("update-category-sort-order")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateCategorySortOrder([FromBody] OnboardingCategorySortRequest req, CancellationToken ct)
    {
        if (req.Items.Count > 0)
        {
            await mediator.Send(new Application.Categories.Commands.UpdateCategorySortOrderCommand
            {
                Items = req.Items
            }, ct);
        }

        await SyncStoreMenuIfRequested(req.SourceMenuId, req.StoreId, ct);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Mark the wizard as completed (called when user clicks 'Finish' on the category step).
    /// </summary>
    [HttpPost("complete")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest? req, CancellationToken ct)
    {
        if (req is not null)
        {
            await SyncStoreMenuIfRequested(req.SourceMenuId, req.StoreId, ct);
        }

        await mediator.Send(new Application.Owners.Commands.MarkWizardCompletedCommand(), ct);
        return Ok(new { success = true });
    }

    private async Task SyncStoreMenuIfRequested(Guid sourceMenuId, Guid? storeId, CancellationToken ct)
    {
        if (sourceMenuId == Guid.Empty || !storeId.HasValue || storeId.Value == Guid.Empty)
            return;

        await mediator.Send(new SyncMainMenuToStoreMenuCommand
        {
            SourceMenuId = sourceMenuId,
            StoreId = storeId.Value
        }, ct);
    }
}

public class AddOnboardingCategoryRequest
{
    public Guid MenuId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid CategoryLibraryItemId { get; set; }
    public int SortOrder { get; set; }
}

public class OnboardingCategorySortRequest
{
    public Guid SourceMenuId { get; set; }
    public Guid? StoreId { get; set; }
    public List<Application.Categories.Commands.CategorySortItem> Items { get; set; } = [];
}

public class CompleteOnboardingRequest
{
    public Guid SourceMenuId { get; set; }
    public Guid? StoreId { get; set; }
}

public class OnboardingCreateMenuRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid? StoreId { get; set; }
}
