namespace TicketingSystem.Models;

public enum TicketPriority
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    WaitingOnUser = 2,
    Resolved = 3,
    Closed = 4
}
