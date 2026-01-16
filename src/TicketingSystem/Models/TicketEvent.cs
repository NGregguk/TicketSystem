using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class TicketEvent
{
    public int Id { get; set; }

    [Required]
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [Required]
    public string ActorUserId { get; set; } = string.Empty;
    public ApplicationUser? ActorUser { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
