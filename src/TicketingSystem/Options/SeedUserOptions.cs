namespace TicketingSystem.Options;

public class SeedUserOptions
{
    public const string SectionName = "SeedUsers";

    public string AdminEmail { get; set; } = "admin@local.test";
    public string AdminPassword { get; set; } = "Admin123!";
    public string RequesterEmail { get; set; } = "requester@local.test";
    public string RequesterPassword { get; set; } = "Requester123!";
}
