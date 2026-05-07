using Application.Companies.Commands;
using Application.Companies.DTOs;
using Application.Companies.Queries;
using Application.MenuDesigns.Commands;
using Application.MenuDesigns.Queries;
using Application.Menus.Commands;
using Application.Menus.Queries;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class MenuController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? search, int page = 1, CancellationToken ct = default)
    {
        if (User.IsInRole("Admin"))
        {
            var menus = await mediator.Send(new GetAllMenusPagedQuery { Search = search, Page = page }, ct);
            return View("AllMenus", menus);
        }
        else
        {
            var menus = await mediator.Send(new GetMenusPagedByCurrentOwnerQuery { Search = search, Page = page, OnlyStoreMenus = true }, ct);
            return View("MyMenus", menus);
        }
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> CompanyMenus([FromQuery] string? search, int page = 1, CancellationToken ct = default)
    {
        if (User.IsInRole("Admin"))
        {
            var menus = await mediator.Send(new GetAllMenusPagedQuery { Search = search, Page = page, OnlyCompanyMenus = true }, ct);
            return View("CompanyMenus", menus);
        }
        else
        {
            var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
            if (company == null) return RedirectToAction("Index", "Onboarding", new { area = "Admin" });

            var menus = await mediator.Send(new GetMenusPagedByCompanyQuery { CompanyId = company.Id, Search = search, Page = page }, ct);
            return View("CompanyMenus", menus);
        }
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> CreateToStore(string? returnUrl, CancellationToken ct)
    {
        var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
        var isSingleStore = ownerCompany?.IsSingleStore == true;
        ViewData["ReturnUrl"] = returnUrl;
        
        ViewBag.IsSingleStore = isSingleStore;
        ViewBag.SingleStoreId = (Guid?)null;
        ViewBag.StoreName = (string?)null;
        
        if (isSingleStore && User.IsInRole("Owner") && ownerCompany != null)
        {
            // Tek dükkan modunda şirketin dükkanını bul
            var storesQuery = new GetStoresPagedQuery
            {
                CompanyId = ownerCompany.Id,
                Page = 1,
                PageSize = 2
            };
            var storesResult = await mediator.Send(storesQuery, ct);
            
            if (storesResult.Items.Count == 1)
            {
                var singleStore = storesResult.Items.First();
                ViewBag.SingleStoreId = singleStore.Id;
                ViewBag.StoreName = singleStore.Title;
                
                // Tek dükkan varsa, model'de storeId'yi önceden doldur
                return View(new CreateMenuToStoreCommand { StoreId = singleStore.Id });
            }
        }
        
        return View(new CreateMenuToStoreCommand());
    }
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateToStore([FromForm] CreateMenuToStoreCommand cmd, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // Tek dükkan modu kontrolü
        var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
        var isSingleStore = ownerCompany?.IsSingleStore == true && User.IsInRole("Owner");
        
        if (isSingleStore)
        {
            // Tek dükkan modunda storeId otomatik olarak ayarlanır
            if (cmd.StoreId == Guid.Empty && ownerCompany != null)
            {
                var storesQuery = new GetStoresPagedQuery
                {
                    CompanyId = ownerCompany.Id,
                    Page = 1,
                    PageSize = 2
                };
                var storesResult = await mediator.Send(storesQuery, ct);
                
                if (storesResult.Items.Count == 1)
                {
                    cmd.StoreId = storesResult.Items.First().Id;
                }
            }
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = cmd.StoreId == Guid.Empty ? (Guid?)null : cmd.StoreId;
            
            if (cmd.StoreId != Guid.Empty)
            {
                // Try to find the store to repopulate the name
                try 
                {
                    var store = await mediator.Send(new GetStoreByIdQuery(cmd.StoreId), ct);
                    ViewBag.StoreName = store.Title;
                }
                catch 
                { 
                    // Ignore if not found, user will just see empty name
                }
            }
            return View(cmd);
        }

        var dto = await mediator.Send(cmd, ct);
        return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id = dto.Id, role = RouteData.Values["role"] }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public IActionResult CreateToCompany(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new CreateMenuToCompanyCommand());
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateToCompany([FromForm] CreateMenuToCompanyCommand cmd, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            if (cmd.CompanyId != Guid.Empty)
            {
                try
                {
                    var company = await mediator.Send(new GetCompanyByIdQuery { Id = cmd.CompanyId }, ct);
                    ViewBag.CompanyName = company.Title;
                }
                catch
                {
                    // Ignore
                }
            }
            return View(cmd);
        }

        var dto = await mediator.Send(cmd, ct);
        return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id = dto.Id, role = RouteData.Values["role"] }));
    }

    	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Details(Guid id, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
		ViewData["ReturnUrl"] = returnUrl;

		if (User.IsInRole("Owner"))
		{
			// Verify ownership
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id)
				{
					isOwner = true;
				}
				else if (menu.StoreId.HasValue)
				{
					// If linked to store, check store's company
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id)
					{
						isOwner = true;
					}
				}
			}

			if (!isOwner) return Forbid();
		}

		ViewBag.MenuDesigns = await mediator.Send(new GetAllMenuDesignsQuery(), ct);
		ViewBag.CurrentMenuDesignId = menu.MenuDesignId;
		return View(menu);
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SetDesign(Guid id, [FromForm] Guid? menuDesignId, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id) isOwner = true;
				else if (menu.StoreId.HasValue)
				{
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id) isOwner = true;
				}
			}
			if (!isOwner) return Forbid();
		}

		await mediator.Send(new SetMenuDesignCommand(id, menuDesignId), ct);
		TempData["Success"] = "Tasarım güncellendi.";
		return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id, role = RouteData.Values["role"] }));
	}





	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Update(Guid id, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
        ViewData["ReturnUrl"] = returnUrl;

		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id) isOwner = true;
				else if (menu.StoreId.HasValue)
				{
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id) isOwner = true;
				}
			}
			if (!isOwner) return Forbid();
		}

		ViewBag.MenuDesigns = await mediator.Send(new GetAllMenuDesignsQuery(), ct);
		ViewBag.CurrentMenuDesignId = menu.MenuDesignId;
		return View(new UpdateMenuCommand(menu.Id, menu.Title));
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Update([FromForm] UpdateMenuCommand req, [FromForm] Guid? menuDesignId, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = req.Id }, ct);
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id) isOwner = true;
				else if (menu.StoreId.HasValue)
				{
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id) isOwner = true;
				}
			}
			if (!isOwner) return Forbid();
		}

		var updatedMenu = await mediator.Send(req, ct);
		await mediator.Send(new SetMenuDesignCommand(updatedMenu.Id, menuDesignId), ct);
		return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id = updatedMenu.Id, role = RouteData.Values["role"] }));
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SetActive(Guid id, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id) isOwner = true;
				else if (menu.StoreId.HasValue)
				{
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id) isOwner = true;
				}
			}
			if (!isOwner) return Forbid();
		}

		await mediator.Send(new SetMenuActiveCommand { Id = id }, ct);
		return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id, role = RouteData.Values["role"] }));
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SetDefault(Guid id, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			var isOwner = ownerCompany != null && menu.CompanyId == ownerCompany.Id;
			if (!isOwner) return Forbid();
		}

		await mediator.Send(new SetDefaultCompanyMenuCommand { Id = id }, ct);
		return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id, role = RouteData.Values["role"] }));
	}
	
	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, string? returnUrl, CancellationToken ct)
	{
		var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			bool isOwner = false;
			if (ownerCompany != null)
			{
				if (menu.CompanyId == ownerCompany.Id) isOwner = true;
				else if (menu.StoreId.HasValue)
				{
					var store = await mediator.Send(new GetStoreByIdQuery(menu.StoreId.Value), ct);
					if (store.CompanyId == ownerCompany.Id) isOwner = true;
				}
			}
			if (!isOwner) return Forbid();
		}

		await mediator.Send(new DeleteMenuCommand { Id = id }, ct);
		return RedirectToLocal(returnUrl, RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] }));
	}    
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> SearchCompanies(string? search, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        IEnumerable<CompanyDTO> items;
        long totalCount = 0;

        if (User.IsInRole("Admin"))
        {
            var result = await mediator.Send(new GetAllCompaniesPagedQuery 
            { 
                Search = search, 
                Page = page, 
                PageSize = pageSize 
            }, ct);
            items = result.Items;
            totalCount = result.Count;
        }
        else
        {
            var result = await mediator.Send(new GetCompaniesPagedByCurrentOwnerQuery 
            { 
                Search = search, 
                Page = page, 
                PageSize = pageSize 
            }, ct);
            items = result.Items;
            totalCount = result.Count;
        }

        return Json(new { items, totalCount });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> SearchStores(string? search, Guid? companyId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetStoresPagedQuery 
        { 
            Search = search, 
            CompanyId = companyId,
            Page = page, 
            PageSize = pageSize 
        }, ct);

        return Json(new { items = result.Items, totalCount = result.Count });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloneToStore([FromForm] CloneMenuToStoreRequest req, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (req.SourceMenuId == Guid.Empty || req.StoreId == Guid.Empty)
        {
            ModelState.AddModelError("", "Kaynak menü ve dükkan seçilmeli.");
            return View("CreateToStore", new CreateMenuToStoreCommand());
        }

        var dto = await mediator.Send(new CloneMenuToStoreCommand
        {
            SourceMenuId = req.SourceMenuId,
            StoreId = req.StoreId
        }, ct);
        return RedirectToLocal(returnUrl, RedirectToAction(nameof(Details), new { id = dto.Id }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> SearchMainMenus(string? search, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var menus = await mediator.Send(new GetMenusPagedByCurrentOwnerQuery
        {
            Search = search,
            Page = page,
            PageSize = pageSize,
            OnlyCompanyMenus = true
        }, ct);

        var items = menus.Items.Select(m => new
        {
            id = m.Id,
            title = m.Title,
            companyName = m.CompanyName,
            isDefaultCompanyMenu = m.IsDefaultCompanyMenu
        });
        return Json(new { items, totalCount = menus.Count });
    }

    [HttpGet]
    public IActionResult DivideByZeroError()
    {
        int zero = 0;
        int result = 1 / zero;
        return View();
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetMenusForSelect2(Guid? companyId, Guid? storeId, string? search, int page = 1, int pageSize = 15, CancellationToken ct = default)
    {
        var result = User.IsInRole("Admin")
            ? await mediator.Send(new GetAllMenusPagedQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                CompanyId = companyId,
                StoreId = storeId
            }, ct)
            : await mediator.Send(new GetMenusPagedByCurrentOwnerQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                CompanyId = companyId,
                StoreId = storeId
            }, ct);

        var items = result.Items.Select(m => new { id = m.Id, text = m.Title });

        return Json(new { results = items, pagination = new { more = result.HasNext } });
    }

    private IActionResult RedirectToLocal(string? returnUrl, IActionResult fallback)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return fallback;
    }
}

public class CloneMenuToStoreRequest
{
    public Guid SourceMenuId { get; set; }
    public Guid StoreId { get; set; }
}

