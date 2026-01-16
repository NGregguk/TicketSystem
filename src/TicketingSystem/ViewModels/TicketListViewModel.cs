using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class TicketListViewModel
{
    public IReadOnlyList<Ticket> Tickets { get; set; } = Array.Empty<Ticket>();
    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> InternalSystems { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Statuses { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Priorities { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Admins { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Requesters { get; set; } = Array.Empty<SelectListItem>();

    public int? CategoryId { get; set; }
    public int? InternalSystemId { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? AssignedAdminUserId { get; set; }
    public string? RequesterUserId { get; set; }
    public string? Search { get; set; }
    public string? Sla { get; set; }

    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
}
