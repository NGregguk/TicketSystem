using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Helpers;
using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.Services;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SlaOptions _slaOptions;
    private readonly TicketAccessService _ticketAccess;

    public ReportsController(ApplicationDbContext db, IOptions<SlaOptions> slaOptions, TicketAccessService ticketAccess)
    {
        _db = db;
        _slaOptions = slaOptions.Value;
        _ticketAccess = ticketAccess;
    }

    public async Task<IActionResult> Index()
    {
        var model = await BuildReportAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var tickets = await GetScopedTickets()
            .Include(t => t.Category)
            .Include(t => t.InternalSystem)
            .Include(t => t.RequesterUser)
            .Include(t => t.AssignedAdminUser)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();

        var timeTotals = await _db.TicketTimeEntries
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .GroupBy(e => e.TicketId)
            .Select(g => new { TicketId = g.Key, Minutes = g.Sum(x => x.Minutes) })
            .ToDictionaryAsync(x => x.TicketId, x => x.Minutes);

        var builder = new StringBuilder();
        builder.AppendLine("Id,Title,Status,Priority,Category,InternalSystem,Requester,AssignedAdmin,CreatedAtUtc,UpdatedAtUtc,ClosedAtUtc,SlaState,TotalTimeMinutes,TotalTimeFormatted");

        foreach (var ticket in tickets)
        {
            var requesterName = ticket.RequesterUser?.DisplayName ?? ticket.RequesterUser?.Email ?? string.Empty;
            var assignedName = ticket.AssignedAdminUser?.DisplayName ?? ticket.AssignedAdminUser?.Email ?? string.Empty;
            var slaState = ticket.Status == TicketStatus.Closed
                ? "Closed"
                : SlaHelper.GetSlaState(ticket.CreatedAtUtc, ticket.Priority, _slaOptions).ToString();
            var totalMinutes = timeTotals.TryGetValue(ticket.Id, out var minutes) ? minutes : 0;
            var totalFormatted = TimeFormatHelper.FormatMinutes(totalMinutes);

            builder.AppendLine(string.Join(",",
                ticket.Id,
                EscapeCsv(ticket.Title),
                ticket.Status,
                ticket.Priority,
                EscapeCsv(ticket.Category?.Name),
                EscapeCsv(ticket.InternalSystem?.Name),
                EscapeCsv(requesterName),
                EscapeCsv(assignedName),
                ticket.CreatedAtUtc.ToString("O"),
                ticket.UpdatedAtUtc.ToString("O"),
                ticket.ClosedAtUtc?.ToString("O") ?? string.Empty,
                slaState,
                totalMinutes,
                EscapeCsv(totalFormatted)));
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"ticket-report-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private async Task<ReportsViewModel> BuildReportAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var startDate = nowUtc.Date.AddDays(-29);
        var endDate = nowUtc.Date;

        var scopedTickets = GetScopedTickets();

        var volumeTickets = await scopedTickets
            .Where(t => t.CreatedAtUtc >= startDate || (t.ClosedAtUtc != null && t.ClosedAtUtc >= startDate))
            .Select(t => new { t.CreatedAtUtc, t.ClosedAtUtc })
            .ToListAsync();

        var dateRange = Enumerable.Range(0, 30)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        var createdLookup = volumeTickets
            .GroupBy(t => t.CreatedAtUtc.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var closedLookup = volumeTickets
            .Where(t => t.ClosedAtUtc.HasValue)
            .GroupBy(t => t.ClosedAtUtc!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var volumeLabels = dateRange.Select(d => d.ToString("MMM d")).ToList();
        var volumeDateKeys = dateRange.Select(d => d.ToString("yyyy-MM-dd")).ToList();
        var volumeCreated = dateRange.Select(d => createdLookup.TryGetValue(d, out var count) ? count : 0).ToList();
        var volumeClosed = dateRange.Select(d => closedLookup.TryGetValue(d, out var count) ? count : 0).ToList();
        var volumeLabels14 = volumeLabels.TakeLast(14).ToList();
        var volumeDateKeys14 = volumeDateKeys.TakeLast(14).ToList();
        var volumeCreated14 = volumeCreated.TakeLast(14).ToList();
        var volumeClosed14 = volumeClosed.TakeLast(14).ToList();

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

        var workloadTickets = await scopedTickets
            .Include(t => t.AssignedAdminUser)
            .Where(t => t.Status != TicketStatus.Closed)
            .ToListAsync();

        var workloadItems = workloadTickets
            .GroupBy(t => t.AssignedAdminUserId)
            .Select(g =>
            {
                var sample = g.FirstOrDefault();
                var name = sample?.AssignedAdminUser?.DisplayName
                           ?? sample?.AssignedAdminUser?.Email
                           ?? (g.Key == null ? "Unassigned" : "Unknown");
                return new ReportsWorkloadItem
                {
                    UserId = g.Key,
                    Name = name,
                    Count = g.Count()
                };
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .ToList();

        var unassignedTickets = await scopedTickets
            .Include(t => t.Category)
            .Where(t => t.AssignedAdminUserId == null && t.Status != TicketStatus.Closed)
            .OrderBy(t => t.CreatedAtUtc)
            .Take(5)
            .Select(t => new ReportsTicketItem
            {
                Id = t.Id,
                Title = t.Title,
                CreatedAtUtc = t.CreatedAtUtc,
                Category = t.Category!.Name
            })
            .ToListAsync();

        var timeEntryQuery = _db.TicketTimeEntries
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.WorkDate >= startDate);

        var ticketIds = await scopedTickets.Select(t => t.Id).ToListAsync();
        timeEntryQuery = timeEntryQuery.Where(e => ticketIds.Contains(e.TicketId));

        var timeLoggedMinutes30 = await timeEntryQuery.SumAsync(e => e.Minutes);

        var topTimeTickets = await timeEntryQuery
            .GroupBy(e => e.TicketId)
            .Select(g => new { TicketId = g.Key, Minutes = g.Sum(x => x.Minutes) })
            .OrderByDescending(x => x.Minutes)
            .Take(10)
            .Join(scopedTickets, g => g.TicketId, t => t.Id, (g, t) => new ReportsTimeTicketItem
            {
                Id = t.Id,
                Title = t.Title,
                Minutes = g.Minutes
            })
            .ToListAsync();

        var totalOpenCount = await scopedTickets.CountAsync(t => t.Status != TicketStatus.Closed);
        var closedLast30Days = await scopedTickets.CountAsync(t => t.ClosedAtUtc != null && t.ClosedAtUtc >= startDate);

        return new ReportsViewModel
        {
            GeneratedAtUtc = nowUtc,
            StartDateUtc = startDate,
            EndDateUtc = endDate,
            TotalOpenCount = totalOpenCount,
            ClosedLast30DaysCount = closedLast30Days,
            OnTrackCount = onTrackCount,
            DueSoonCount = dueSoonCount,
            OverdueCount = overdueCount,
            TimeLoggedMinutes30 = timeLoggedMinutes30,
            TopTimeTickets = topTimeTickets,
            VolumeLabels = volumeLabels,
            VolumeCreatedCounts = volumeCreated,
            VolumeClosedCounts = volumeClosed,
            VolumeLabels14 = volumeLabels14,
            VolumeCreatedCounts14 = volumeCreated14,
            VolumeClosedCounts14 = volumeClosed14,
            VolumeDateKeys = volumeDateKeys,
            VolumeDateKeys14 = volumeDateKeys14,
            WorkloadItems = workloadItems,
            UnassignedTickets = unassignedTickets
        };
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n')
            ? $"\"{escaped}\""
            : escaped;
    }

    private IQueryable<Ticket> GetScopedTickets()
    {
        var userId = _ticketAccess.GetUserId(User) ?? string.Empty;
        var isAdmin = User.IsInRole(RoleNames.Admin);
        return _ticketAccess.ApplyViewScope(_db.Tickets.AsNoTracking(), userId, isAdmin);
    }
}
