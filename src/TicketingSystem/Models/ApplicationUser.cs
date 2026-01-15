using Microsoft.AspNetCore.Identity;

namespace TicketingSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
