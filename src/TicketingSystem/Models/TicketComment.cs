using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [Required]
    public string AuthorUserId { get; set; } = string.Empty;
    public ApplicationUser? AuthorUser { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public bool IsPublic { get; set; } = true;
}
