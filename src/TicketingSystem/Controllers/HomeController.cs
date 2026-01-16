using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Models;
using TicketingSystem.Services;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DashboardService _dashboardService;

    public HomeController(UserManager<ApplicationUser> userManager, DashboardService dashboardService)
    {
        _userManager = userManager;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User) ?? string.Empty;
        var viewModel = await _dashboardService.GetDashboardAsync(new DashboardUserContext
        {
            UserId = userId,
            IsAdmin = User.IsInRole(RoleNames.Admin),
            CanViewAll = true
        });

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
