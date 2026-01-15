using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }
}
