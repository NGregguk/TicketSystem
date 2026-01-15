using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
