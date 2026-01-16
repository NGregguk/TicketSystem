using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.None;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    [Required]
    public string RequesterUserId { get; set; } = string.Empty;
    public ApplicationUser? RequesterUser { get; set; }

    public string? AssignedAdminUserId { get; set; }
    public ApplicationUser? AssignedAdminUser { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public int ReopenCount { get; set; }
    public DateTime? ReopenedAtUtc { get; set; }
    public string? ReopenedByUserId { get; set; }
    public ApplicationUser? ReopenedByUser { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketInternalNote> InternalNotes { get; set; } = new List<TicketInternalNote>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}
