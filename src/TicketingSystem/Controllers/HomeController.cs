using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Helpers;
using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SlaOptions _slaOptions;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IOptions<SlaOptions> slaOptions)
    {
        _db = db;
        _userManager = userManager;
        _slaOptions = slaOptions.Value;
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

        var slaCandidates = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Status != TicketStatus.Closed)
            .Select(t => new { t.CreatedAtUtc, t.Priority })
            .ToListAsync();

        var dueSoonCount = 0;
        var overdueCount = 0;
        foreach (var ticket in slaCandidates)
        {
            var slaState = SlaHelper.GetSlaState(ticket.CreatedAtUtc, ticket.Priority, _slaOptions);
            if (slaState == SlaState.Overdue)
            {
                overdueCount++;
            }
            else if (slaState == SlaState.DueSoon)
            {
                dueSoonCount++;
            }
        }

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
            NeedsAttentionTickets = needsAttention,
            DueSoonCount = dueSoonCount,
            OverdueCount = overdueCount
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
