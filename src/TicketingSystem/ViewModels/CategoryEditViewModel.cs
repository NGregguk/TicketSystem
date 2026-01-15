using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.ViewModels;

public class CategoryEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
