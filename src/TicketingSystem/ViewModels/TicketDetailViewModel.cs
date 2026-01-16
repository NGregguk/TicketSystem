using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class TicketDetailViewModel
{
    public Ticket Ticket { get; set; } = null!;
    public IReadOnlyList<TicketComment> Comments { get; set; } = Array.Empty<TicketComment>();
    public IReadOnlyList<TicketInternalNote> InternalNotes { get; set; } = Array.Empty<TicketInternalNote>();
    public IReadOnlyList<TicketAttachment> Attachments { get; set; } = Array.Empty<TicketAttachment>();
    public ISet<int> AllowedAttachmentIds { get; set; } = new HashSet<int>();

    public string NewComment { get; set; } = string.Empty;

    public string? NewInternalNote { get; set; }

    public string? AssignToUserId { get; set; }
    public TicketStatus? NewStatus { get; set; }
    public TicketPriority? NewPriority { get; set; }
    public int? NewCategoryId { get; set; }
    public int? NewInternalSystemId { get; set; }

    public IEnumerable<SelectListItem> Admins { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Statuses { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Priorities { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> InternalSystems { get; set; } = Array.Empty<SelectListItem>();
}
