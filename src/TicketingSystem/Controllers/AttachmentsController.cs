using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.Services;

namespace TicketingSystem.Controllers;

[Authorize]
[Route("attachments")]
public class AttachmentsController : Controller
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly UploadOptions _uploadOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        ApplicationDbContext db,
        IFileStorage fileStorage,
        IOptions<UploadOptions> uploadOptions,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager,
        ILogger<AttachmentsController> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _uploadOptions = uploadOptions.Value;
        _environment = environment;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("upload-temp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadTemp(IFormFile file, string tempKey)
    {
        if (!ValidateImage(file, out var error))
        {
            return BadRequest(new { error });
        }

        if (string.IsNullOrWhiteSpace(tempKey))
        {
            return BadRequest(new { error = "Missing temp key." });
        }

        var userId = _userManager.GetUserId(User) ?? string.Empty;
        var subfolder = Path.Combine("temp", tempKey);
        var storedFileName = await _fileStorage.SaveAsync(file, subfolder, HttpContext.RequestAborted);

        var attachment = new TicketAttachment
        {
            TicketId = null,
            TempKey = tempKey,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = userId
        };

        _db.TicketAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Temporary attachment {AttachmentId} uploaded by {UserId}", attachment.Id, userId);

        return Ok(new
        {
            attachmentId = attachment.Id,
            fileName = attachment.OriginalFileName,
            url = Url.Action("ViewAttachment", new { id = attachment.Id })
        });
    }

    [HttpPost("upload-ticket/{ticketId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadToTicket(int ticketId, IFormFile file)
    {
        if (!ValidateImage(file, out var error))
        {
            return BadRequest(new { error });
        }

        var ticket = await _db.Tickets
            .Include(t => t.RequesterUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
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

        var subfolder = Path.Combine("tickets", ticketId.ToString());
        var storedFileName = await _fileStorage.SaveAsync(file, subfolder, HttpContext.RequestAborted);
        var attachment = new TicketAttachment
        {
            TicketId = ticketId,
            TempKey = null,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = _userManager.GetUserId(User) ?? string.Empty
        };

        _db.TicketAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Attachment {AttachmentId} uploaded for ticket {TicketId}", attachment.Id, ticketId);

        return Ok(new
        {
            attachmentId = attachment.Id,
            fileName = attachment.OriginalFileName,
            url = Url.Action("ViewAttachment", new { id = attachment.Id })
        });
    }

    [HttpGet("view/{id:int}")]
    public async Task<IActionResult> ViewAttachment(int id)
    {
        var attachment = await _db.TicketAttachments
            .Include(a => a.Ticket)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (attachment == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User) ?? string.Empty;
        if (!attachment.TicketId.HasValue &&
            !string.Equals(attachment.UploadedByUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
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

        return PhysicalFile(fullPath, contentType);
    }

    private bool ValidateImage(IFormFile file, out string error)
    {
        error = string.Empty;

        if (file == null || file.Length <= 0)
        {
            error = "File is empty.";
            return false;
        }

        if (file.Length > _uploadOptions.MaxSizeBytes)
        {
            error = $"File exceeds {_uploadOptions.MaxSizeBytes / (1024 * 1024)} MB limit.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExtensions.Contains(ext))
        {
            error = "Only image files are allowed for paste uploads.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            error = "Only image files are allowed for paste uploads.";
            return false;
        }

        return true;
    }
}
