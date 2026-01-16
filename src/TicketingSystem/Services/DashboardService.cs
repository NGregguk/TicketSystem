using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Helpers;
using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly SlaOptions _slaOptions;

    public DashboardService(ApplicationDbContext db, IOptions<SlaOptions> slaOptions)
    {
        _db = db;
        _slaOptions = slaOptions.Value;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(DashboardUserContext user)
    {
        var nowUtc = DateTime.UtcNow;
        var startDate = nowUtc.Date.AddDays(-29);

        var scopedTickets = ApplyScope(_db.Tickets.AsNoTracking(), user);
        var showGlobal = user.CanViewAll;

        var statusCounts = await scopedTickets
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var unassignedCount = 0;
        if (showGlobal)
        {
            unassignedCount = await scopedTickets
                .Where(t => t.AssignedAdminUserId == null && t.Status != TicketStatus.Closed)
                .CountAsync();
        }

        var statusCards = new List<DashboardMetricCard>();
        if (showGlobal)
        {
            statusCards.Add(BuildCard("unassigned", "Unassigned", unassignedCount, "?assignedAdminUserId=unassigned"));
        }

        statusCards.AddRange(new[]
        {
            BuildCard("open", "Open", statusCounts.GetValueOrDefault(TicketStatus.Open), "?status=Open"),
            BuildCard("inprogress", "In Progress", statusCounts.GetValueOrDefault(TicketStatus.InProgress), "?status=InProgress"),
            BuildCard("waiting", "Waiting on User", statusCounts.GetValueOrDefault(TicketStatus.WaitingOnUser), "?status=WaitingOnUser"),
            BuildCard("closed", "Closed", statusCounts.GetValueOrDefault(TicketStatus.Closed), "?status=Closed")
        });

        var slaCandidates = await scopedTickets
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

        var slaCards = new List<DashboardMetricCard>
        {
            BuildCard("ontrack", "On Track", onTrackCount, "?sla=ontrack"),
            BuildCard("due", "Due Soon", dueSoonCount, "?sla=due"),
            BuildCard("overdue", "Overdue", overdueCount, "?sla=overdue")
        };

        var volumeTickets = await scopedTickets
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

        var categoryCounts = await scopedTickets
            .Include(t => t.Category)
            .Where(t => t.Status != TicketStatus.Closed)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var categoryLabels = categoryCounts.Select(x => x.Name).ToList();
        var categoryIds = categoryCounts.Select(x => x.CategoryId).ToList();
        var categoryData = categoryCounts.Select(x => x.Count).ToList();

        var myOpenTicketsQuery = _db.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.RequesterUser)
            .Where(t => t.Status != TicketStatus.Closed);

        if (user.IsAdmin)
        {
            myOpenTicketsQuery = myOpenTicketsQuery.Where(t => t.AssignedAdminUserId == user.UserId);
        }
        else
        {
            myOpenTicketsQuery = myOpenTicketsQuery.Where(t => t.RequesterUserId == user.UserId);
        }

        var myOpenTickets = await myOpenTicketsQuery
            .OrderByDescending(t => t.UpdatedAtUtc)
            .Take(20)
            .ToListAsync();

        var needsAttentionTickets = new List<Ticket>();

        if (user.IsAdmin)
        {
            needsAttentionTickets = await scopedTickets
                .Include(t => t.Category)
                .Where(t => t.Status == TicketStatus.WaitingOnUser
                            || t.Priority == TicketPriority.High
                            || t.Priority == TicketPriority.Critical)
                .Where(t => t.Status != TicketStatus.Closed)
                .OrderByDescending(t => t.UpdatedAtUtc)
                .Take(5)
                .ToListAsync();
        }
        else
        {
            needsAttentionTickets = await _db.Tickets
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.RequesterUserId == user.UserId)
                .Where(t => t.Status == TicketStatus.WaitingOnUser)
                .OrderByDescending(t => t.UpdatedAtUtc)
                .Take(5)
                .ToListAsync();
        }

        return new DashboardViewModel
        {
            IsAdmin = user.IsAdmin,
            Title = "Dashboard",
            Subtitle = showGlobal
                ? "A quick view of ticket health and what needs attention."
                : "A quick view of your ticket health and updates.",
            ScopeNote = showGlobal ? null : "Showing your tickets only.",
            StatusCards = statusCards,
            SlaCards = slaCards,
            MyOpenTicketsTitle = user.IsAdmin ? "My Open Tickets" : "Your Open Tickets",
            MyOpenTickets = myOpenTickets
                .OrderBy(t =>
                {
                    var slaState = SlaHelper.GetSlaState(t.CreatedAtUtc, t.Priority, _slaOptions);
                    return slaState switch
                    {
                        SlaState.Overdue => 0,
                        SlaState.DueSoon => 1,
                        _ => 2
                    };
                })
                .ThenBy(t => t.Priority switch
                {
                    TicketPriority.Critical => 0,
                    TicketPriority.High => 1,
                    TicketPriority.Medium => 2,
                    TicketPriority.Low => 3,
                    _ => 4
                })
                .ThenBy(t => t.CreatedAtUtc)
                .Take(8)
                .ToList(),
            NeedsAttentionTickets = needsAttentionTickets,
            NeedsAttentionTitle = user.IsAdmin ? "Needs Attention" : "Tickets requiring your response",
            NeedsAttentionEmptyTitle = user.IsAdmin ? "No high priority items" : "You're all caught up",
            NeedsAttentionEmptyMessage = user.IsAdmin
                ? "Waiting on user or high priority tickets will show up here."
                : "We will highlight tickets that need your input.",
            OnTrackCount = onTrackCount,
            DueSoonCount = dueSoonCount,
            OverdueCount = overdueCount,
            VolumeLabels14 = volumeLabels14,
            VolumeCreatedCounts14 = volumeCreated14,
            VolumeClosedCounts14 = volumeClosed14,
            VolumeLabels30 = volumeLabels30,
            VolumeCreatedCounts30 = volumeCreated30,
            VolumeClosedCounts30 = volumeClosed30,
            VolumeDateKeys14 = volumeDateKeys14,
            VolumeDateKeys30 = volumeDateKeys30,
            CategoryLabels = categoryLabels,
            CategoryCounts = categoryData,
            CategoryIds = categoryIds
        };
    }

    // All dashboard metrics must use ApplyScope(user) to avoid leaking data between roles.
    private static IQueryable<Ticket> ApplyScope(IQueryable<Ticket> query, DashboardUserContext user)
    {
        return user.CanViewAll ? query : query.Where(t => t.RequesterUserId == user.UserId);
    }

    private static DashboardMetricCard BuildCard(string key, string label, int count, string filterQueryString)
    {
        return new DashboardMetricCard
        {
            Key = key,
            Label = label,
            Count = count,
            FilterQueryString = filterQueryString
        };
    }
}

public class DashboardUserContext
{
    public string UserId { get; init; } = string.Empty;
    public bool IsAdmin { get; init; }
    public bool CanViewAll { get; init; }
}
