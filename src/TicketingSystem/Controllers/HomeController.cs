using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Data;
using TicketingSystem.Models;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var statusCounts = await _db.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var userId = _userManager.GetUserId(User) ?? string.Empty;

        var myOpenTickets = await _db.Tickets
            .Include(t => t.Category)
            .Where(t => t.RequesterUserId == userId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(5)
            .ToListAsync();

        var unassignedTickets = new List<Ticket>();
        var needsAttention = new List<Ticket>();
        if (User.IsInRole(RoleNames.Admin))
        {
            unassignedTickets = await _db.Tickets
                .Include(t => t.Category)
                .Where(t => t.AssignedAdminUserId == null && t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Take(5)
                .ToListAsync();

            needsAttention = await _db.Tickets
                .Include(t => t.Category)
                .Where(t => t.Status == TicketStatus.WaitingOnUser
                            || t.Priority == TicketPriority.High
                            || t.Priority == TicketPriority.Critical)
                .Where(t => t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.UpdatedAtUtc)
                .Take(5)
                .ToListAsync();
        }

        var viewModel = new DashboardViewModel
        {
            StatusCounts = statusCounts,
            MyOpenTickets = myOpenTickets,
            UnassignedTickets = unassignedTickets,
            NeedsAttentionTickets = needsAttention
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
