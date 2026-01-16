using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class TicketTimeEntry
{
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 1440)]
    public int Minutes { get; set; }

    public DateTime WorkDate { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }
}
