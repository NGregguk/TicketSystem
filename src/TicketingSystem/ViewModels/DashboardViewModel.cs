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
    public string MyOpenTicketsTitle { get; set; } = "My Open Tickets";
    public IReadOnlyList<Ticket> MyOpenTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> NeedsAttentionTickets { get; set; } = Array.Empty<Ticket>();
    public string NeedsAttentionTitle { get; set; } = "Needs Attention";
    public string NeedsAttentionEmptyTitle { get; set; } = "No items to review";
    public string NeedsAttentionEmptyMessage { get; set; } = "Everything looks under control right now.";
    public IReadOnlyList<Ticket> SubscribedTickets { get; set; } = Array.Empty<Ticket>();
    public string SubscribedTicketsTitle { get; set; } = "Subscribed Tickets";
    public string SubscribedTicketsEmptyTitle { get; set; } = "No subscribed tickets";
    public string SubscribedTicketsEmptyMessage { get; set; } = "Tickets you follow will appear here.";
    public int DueSoonCount { get; set; }
    public int OverdueCount { get; set; }
    public int OnTrackCount { get; set; }
    public int MyDueSoonCount { get; set; }
    public int MyOverdueCount { get; set; }
    public int MyOnTrackCount { get; set; }
    public IReadOnlyList<string> VolumeLabels14 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> VolumeCreatedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> VolumeClosedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<string> VolumeLabels30 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> VolumeCreatedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> VolumeClosedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MyVolumeCreatedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MyVolumeClosedCounts14 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MyVolumeCreatedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MyVolumeClosedCounts30 { get; set; } = Array.Empty<int>();
    public IReadOnlyList<string> VolumeDateKeys14 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> VolumeDateKeys30 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CategoryLabels { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> CategoryCounts { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> CategoryIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<string> MyCategoryLabels { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> MyCategoryCounts { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MyCategoryIds { get; set; } = Array.Empty<int>();
}

public class DashboardMetricCard
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? FilterQueryString { get; set; }
    public bool IsClickable { get; set; } = true;
}
