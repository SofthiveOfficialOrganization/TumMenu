using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using WebUI.Models;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        
        // AUTO-FIX: Check specific user roles and fix if needed
        var targetEmail = "aemir@gmail.com";
        var user = await _userManager.FindByEmailAsync(targetEmail);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"DEBUG ROLE CHECK for {targetEmail}: {string.Join(", ", roles)}");

            if (roles.Contains("Owner") && roles.Contains("Admin"))
            {
                 Console.WriteLine($"FIXING ROLES for {targetEmail}: Removing Owner role...");
                 await _userManager.RemoveFromRoleAsync(user, "Owner");
                 Console.WriteLine("FIX COMPLETE. Owner role removed.");
            }
        }

        return View();
    }
}

