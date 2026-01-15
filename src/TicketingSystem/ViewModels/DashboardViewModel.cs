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
    public IReadOnlyList<string> VolumeLabels { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> VolumeCreatedCounts { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> VolumeClosedCounts { get; set; } = Array.Empty<int>();
}
