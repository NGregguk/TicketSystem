using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models;

public class TicketAttachment
{
    public int Id { get; set; }

    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [MaxLength(64)]
    public string? TempKey { get; set; }

    [Required]
    [MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    public DateTime UploadedAtUtc { get; set; }

    [Required]
    public string UploadedByUserId { get; set; } = string.Empty;
    public ApplicationUser? UploadedByUser { get; set; }
}
