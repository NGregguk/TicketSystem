using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class DashboardViewModel
{
    public IDictionary<TicketStatus, int> StatusCounts { get; set; } = new Dictionary<TicketStatus, int>();
    public IReadOnlyList<Ticket> MyOpenTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> UnassignedTickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Ticket> NeedsAttentionTickets { get; set; } = Array.Empty<Ticket>();
}
