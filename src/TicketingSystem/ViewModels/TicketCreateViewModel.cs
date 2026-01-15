using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.Models;

namespace TicketingSystem.ViewModels;

public class TicketCreateViewModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int? CategoryId { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.None;

    public IFormFile? Attachment { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();
}
