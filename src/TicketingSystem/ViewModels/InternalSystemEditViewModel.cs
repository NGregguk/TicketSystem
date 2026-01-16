using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.ViewModels;

public class InternalSystemEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
