using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Helpers;
using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.Services;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<TicketsController> _logger;
    private readonly SlaOptions _slaOptions;
    private readonly UploadOptions _uploadOptions;
    private readonly IWebHostEnvironment _environment;

    public TicketsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IFileStorage fileStorage,
        ILogger<TicketsController> logger,
        IOptions<SlaOptions> slaOptions,
        IOptions<UploadOptions> uploadOptions,
        IWebHostEnvironment environment)
    {
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _fileStorage = fileStorage;
        _logger = logger;
        _slaOptions = slaOptions.Value;
        _uploadOptions = uploadOptions.Value;
        _environment = environment;
    }

    public async Task<IActionResult> Index(
        int? categoryId,
        TicketStatus? status,
        TicketPriority? priority,
        string? assignedAdminUserId,
        string? search,
        string? sla,
        int page = 1)
    {
        var query = _db.Tickets
            .Include(t => t.Category)
            .Include(t => t.RequesterUser)
            .Include(t => t.AssignedAdminUser)
            .AsQueryable();

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            query = query.Where(t => t.RequesterUserId == userId);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(assignedAdminUserId))
        {
            query = query.Where(t => t.AssignedAdminUserId == assignedAdminUserId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            if (int.TryParse(search, out var ticketId))
            {
                query = query.Where(t => t.Id == ticketId || t.Title.Contains(search));
            }
            else
            {
                query = query.Where(t => t.Title.Contains(search));
            }
        }

        if (!string.IsNullOrWhiteSpace(sla))
        {
            var slaCandidates = await query
                .Select(t => new { t.Id, t.CreatedAtUtc, t.Priority, t.Status })
                .AsNoTracking()
                .ToListAsync();

            var matchingIds = slaCandidates
                .Where(t => t.Status != TicketStatus.Closed)
                .Where(t =>
                {
                    var state = SlaHelper.GetSlaState(t.CreatedAtUtc, t.Priority, _slaOptions);
                    return sla.Equals("due", StringComparison.OrdinalIgnoreCase)
                        ? state == SlaState.DueSoon
                        : sla.Equals("overdue", StringComparison.OrdinalIgnoreCase) && state == SlaState.Overdue;
                })
                .Select(t => t.Id)
                .ToList();

            query = query.Where(t => matchingIds.Contains(t.Id));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var tickets = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var adminUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);

        var viewModel = new TicketListViewModel
        {
            Tickets = tickets,
            Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())),
            Statuses = Enum.GetValues<TicketStatus>().Select(s => new SelectListItem(s.ToString(), s.ToString())),
            Priorities = Enum.GetValues<TicketPriority>().Select(p => new SelectListItem(p.ToString(), p.ToString())),
            Admins = adminUsers.Select(u => new SelectListItem(u.DisplayName ?? u.Email, u.Id)),
            CategoryId = categoryId,
            Status = status,
            Priority = priority,
            AssignedAdminUserId = assignedAdminUserId,
            Search = search,
            Sla = sla,
            Page = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var viewModel = new TicketCreateViewModel
        {
            Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())),
            Priority = TicketPriority.Medium
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateViewModel model)
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        model.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString()));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User) ?? string.Empty;
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CategoryId = model.CategoryId!.Value,
            Priority = model.Priority,
            Status = TicketStatus.Open,
            RequesterUserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        if (model.Attachment != null)
        {
            if (_fileStorage.IsAllowed(model.Attachment, out var error))
            {
                var storedFileName = await _fileStorage.SaveAsync(model.Attachment, HttpContext.RequestAborted);
                var attachment = new TicketAttachment
                {
                    TicketId = ticket.Id,
                    OriginalFileName = model.Attachment.FileName,
                    StoredFileName = storedFileName,
                    ContentType = model.Attachment.ContentType,
                    SizeBytes = model.Attachment.Length,
                    UploadedAtUtc = now,
                    UploadedByUserId = userId
                };

                _db.TicketAttachments.Add(attachment);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Attachment uploaded for ticket {TicketId}", ticket.Id);
                TempData["Success"] = "Attachment uploaded.";
            }
            else
            {
                TempData["UploadError"] = error ?? "Attachment upload failed.";
            }
        }

        _logger.LogInformation("Ticket {TicketId} created by {UserId}", ticket.Id, userId);
        await _emailSender.SendTicketCreatedAsync(ticket);
        TempData["Success"] = "Ticket created successfully.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Category)
            .Include(t => t.RequesterUser)
            .Include(t => t.AssignedAdminUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.AuthorUser)
            .Include(t => t.InternalNotes)
                .ThenInclude(n => n.AuthorUser)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.UploadedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            if (ticket.RequesterUserId != userId)
            {
                return Forbid();
            }
        }

        var adminUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);

        var viewModel = new TicketDetailViewModel
        {
            Ticket = ticket,
            Comments = ticket.Comments.OrderBy(c => c.CreatedAtUtc).ToList(),
            InternalNotes = ticket.InternalNotes.OrderByDescending(n => n.CreatedAtUtc).ToList(),
            Attachments = ticket.Attachments.OrderByDescending(a => a.UploadedAtUtc).ToList(),
            Admins = adminUsers.Select(u => new SelectListItem(u.DisplayName ?? u.Email, u.Id, u.Id == ticket.AssignedAdminUserId)),
            Statuses = Enum.GetValues<TicketStatus>().Select(s => new SelectListItem(s.ToString(), s.ToString(), s == ticket.Status)),
            Priorities = Enum.GetValues<TicketPriority>().Select(p => new SelectListItem(p.ToString(), p.ToString(), p == ticket.Priority))
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, TicketDetailViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewComment))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            if (ticket.RequesterUserId != userId)
            {
                return Forbid();
            }
        }

        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = _userManager.GetUserId(User) ?? string.Empty,
            Body = model.NewComment.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            IsPublic = true
        };

        _db.TicketComments.Add(comment);
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Comment added to ticket {TicketId}", ticket.Id);
        await _emailSender.SendNewCommentAsync(ticket, comment);
        TempData["Success"] = "Comment posted.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(int id, int commentId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Comment cannot be empty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var comment = await _db.TicketComments
            .Include(c => c.Ticket)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.TicketId == id);
        if (comment == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;
        var isAdmin = User.IsInRole(RoleNames.Admin);
        if (!isAdmin && comment.AuthorUserId != currentUserId)
        {
            return Forbid();
        }

        comment.Body = body.Trim();
        comment.Ticket!.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated on ticket {TicketId}", comment.Id, comment.TicketId);
        TempData["Success"] = "Comment updated.";

        return RedirectToAction(nameof(Details), new { id = comment.TicketId });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInternalNote(int id, TicketDetailViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewInternalNote))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        var note = new TicketInternalNote
        {
            TicketId = ticket.Id,
            AuthorUserId = _userManager.GetUserId(User) ?? string.Empty,
            Body = model.NewInternalNote.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.TicketInternalNotes.Add(note);
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Internal note added to ticket {TicketId}", ticket.Id);
        TempData["Success"] = "Internal note saved.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInternalNote(int id, int noteId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Internal note cannot be empty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var note = await _db.TicketInternalNotes
            .Include(n => n.Ticket)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.TicketId == id);
        if (note == null)
        {
            return NotFound();
        }

        note.Body = body.Trim();
        note.Ticket!.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Internal note {NoteId} updated on ticket {TicketId}", note.Id, note.TicketId);
        TempData["Success"] = "Internal note updated.";

        return RedirectToAction(nameof(Details), new { id = note.TicketId });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TicketDetailViewModel model)
    {
        if (!model.NewStatus.HasValue)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        var oldStatus = ticket.Status;
        ticket.Status = model.NewStatus.Value;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        if (ticket.Status == TicketStatus.Closed)
        {
            ticket.ClosedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Ticket {TicketId} status changed from {OldStatus} to {NewStatus}", ticket.Id, oldStatus, ticket.Status);
        await _emailSender.SendTicketStatusChangedAsync(ticket, oldStatus, ticket.Status);
        TempData["Success"] = "Status updated.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePriority(int id, TicketDetailViewModel model)
    {
        if (!model.NewPriority.HasValue)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        ticket.Priority = model.NewPriority.Value;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} priority changed to {Priority}", ticket.Id, ticket.Priority);
        TempData["Success"] = "Priority updated.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, TicketDetailViewModel model)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        ticket.AssignedAdminUserId = string.IsNullOrWhiteSpace(model.AssignToUserId) ? null : model.AssignToUserId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} assigned to {AssignedUserId}", ticket.Id, ticket.AssignedAdminUserId ?? "Unassigned");
        await _emailSender.SendTicketAssignedAsync(ticket);
        TempData["Success"] = "Assignment updated.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            if (ticket.RequesterUserId != userId)
            {
                return Forbid();
            }
        }

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAtUtc = DateTime.UtcNow;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} closed", ticket.Id);
        await _emailSender.SendTicketStatusChangedAsync(ticket, oldStatus, ticket.Status);
        TempData["Success"] = "Ticket closed.";

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile attachment)
    {
        if (attachment == null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            if (ticket.RequesterUserId != userId)
            {
                return Forbid();
            }
        }

        if (!_fileStorage.IsAllowed(attachment, out var error))
        {
            TempData["UploadError"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        var storedFileName = await _fileStorage.SaveAsync(attachment, HttpContext.RequestAborted);
        var attachmentEntity = new TicketAttachment
        {
            TicketId = ticket.Id,
            OriginalFileName = attachment.FileName,
            StoredFileName = storedFileName,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = _userManager.GetUserId(User) ?? string.Empty
        };

        _db.TicketAttachments.Add(attachmentEntity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Attachment uploaded for ticket {TicketId}", ticket.Id);
        TempData["Success"] = "Attachment uploaded.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RequesterUser)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            if (ticket.RequesterUserId != userId)
            {
                return Forbid();
            }
        }

        var attachment = await _db.TicketAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketId == id);
        if (attachment == null)
        {
            return NotFound();
        }

        var root = Path.IsPathRooted(_uploadOptions.RootPath)
            ? _uploadOptions.RootPath
            : Path.Combine(_environment.ContentRootPath, _uploadOptions.RootPath);

        var fullPath = Path.Combine(root, attachment.StoredFileName);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var contentType = string.IsNullOrWhiteSpace(attachment.ContentType)
            ? "application/octet-stream"
            : attachment.ContentType;

        return PhysicalFile(fullPath, contentType, attachment.OriginalFileName);
    }
}
