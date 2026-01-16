namespace TicketingSystem.ViewModels;

public class TicketSubscriberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AddedByName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
