using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class DashboardViewModel
{
    public bool IsAdmin { get; set; }
    public string Title { get; set; } = "Dashboard";
    public string Subtitle { get; set; } = string.Empty;
    public string? ScopeNote { get; set; }
    public IReadOnlyList<DashboardMetricCard> StatusCards { get; set; } = Array.Empty<DashboardMetricCard>();
    public IReadOnlyList<DashboardMetricCard> SlaCards { get; set; } = Array.Empty<DashboardMetricCard>();
    public IReadOnlyList<Ticket> MyOpenTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> UnassignedTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> NeedsAttentionTickets { get; set; } = Array.Empty<Ticket>();
    public string NeedsAttentionTitle { get; set; } = "Needs Attention";
    public string NeedsAttentionEmptyTitle { get; set; } = "No items to review";
    public string NeedsAttentionEmptyMessage { get; set; } = "Everything looks under control right now.";
    public int DueSoonCount { get; set; }
    public int OverdueCount { get; set; }
    public int OnTrackCount { get; set; }
    public IReadOnlyList<string> VolumeLabels14 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> VolumeCreatedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> VolumeClosedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<string> VolumeLabels30 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> VolumeCreatedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> VolumeClosedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<string> VolumeDateKeys14 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> VolumeDateKeys30 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CategoryLabels { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> CategoryCounts { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> CategoryIds { get; set; } = Array.Empty<int>();
}

public class DashboardMetricCard
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? FilterQueryString { get; set; }
    public bool IsClickable { get; set; } = true;
}
