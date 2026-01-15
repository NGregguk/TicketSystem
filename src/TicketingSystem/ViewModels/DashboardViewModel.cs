using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class DashboardViewModel
{
    public IDictionary<TicketStatus, int> StatusCounts { get; set; } = new Dictionary<TicketStatus, int>();
    public IReadOnlyList<Ticket> MyOpenTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> UnassignedTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> NeedsAttentionTickets { get; set; } = Array.Empty<Ticket>();
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
}
