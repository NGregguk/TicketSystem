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

        var nowUtc = DateTime.UtcNow;
        var startDate = nowUtc.Date.AddDays(-29);
        var volumeTickets = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.CreatedAtUtc >= startDate || (t.ClosedAtUtc != null && t.ClosedAtUtc >= startDate))
            .Select(t => new { t.CreatedAtUtc, t.ClosedAtUtc })
            .ToListAsync();

        var dateRange30 = Enumerable.Range(0, 30)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        var createdLookup = volumeTickets
            .GroupBy(t => t.CreatedAtUtc.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var closedLookup = volumeTickets
            .Where(t => t.ClosedAtUtc.HasValue)
            .GroupBy(t => t.ClosedAtUtc!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var volumeLabels30 = dateRange30.Select(d => d.ToString("MMM d")).ToList();
        var volumeDateKeys30 = dateRange30.Select(d => d.ToString("yyyy-MM-dd")).ToList();
        var volumeCreated30 = dateRange30.Select(d => createdLookup.TryGetValue(d, out var count) ? count : 0).ToList();
        var volumeClosed30 = dateRange30.Select(d => closedLookup.TryGetValue(d, out var count) ? count : 0).ToList();

        var volumeLabels14 = volumeLabels30.TakeLast(14).ToList();
        var volumeDateKeys14 = volumeDateKeys30.TakeLast(14).ToList();
        var volumeCreated14 = volumeCreated30.TakeLast(14).ToList();
        var volumeClosed14 = volumeClosed30.TakeLast(14).ToList();

        var categoryCounts = await _db.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.Status != TicketStatus.Closed)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var categoryLabels = categoryCounts.Select(x => x.Category).ToList();
        var categoryData = categoryCounts.Select(x => x.Count).ToList();

        var slaCandidates = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Status != TicketStatus.Closed)
            .Select(t => new { t.CreatedAtUtc, t.Priority })
            .ToListAsync();

        var dueSoonCount = 0;
        var overdueCount = 0;
        var onTrackCount = 0;
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
            else
            {
                onTrackCount++;
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
            OverdueCount = overdueCount,
            OnTrackCount = onTrackCount,
            VolumeLabels14 = volumeLabels14,
            VolumeCreatedCounts14 = volumeCreated14,
            VolumeClosedCounts14 = volumeClosed14,
            VolumeLabels30 = volumeLabels30,
            VolumeCreatedCounts30 = volumeCreated30,
            VolumeClosedCounts30 = volumeClosed30,
            VolumeDateKeys14 = volumeDateKeys14,
            VolumeDateKeys30 = volumeDateKeys30,
            CategoryLabels = categoryLabels,
            CategoryCounts = categoryData
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
