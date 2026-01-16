using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Data;
using TicketingSystem.Models;
using TicketingSystem.Options;

namespace TicketingSystem.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly NotificationOptions _notificationOptions;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IOptions<NotificationOptions> notificationOptions,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<SmtpEmailSender> logger)
    {
        _db = db;
        _userManager = userManager;
        _notificationOptions = notificationOptions.Value;
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendTicketCreatedAsync(Ticket ticket)
    {
        var requesterEmail = await GetUserEmailAsync(ticket.RequesterUserId);
        var recipients = new List<string>();
        if (!string.IsNullOrWhiteSpace(_notificationOptions.AdminMailbox))
        {
            recipients.Add(_notificationOptions.AdminMailbox!);
        }

        if (!string.IsNullOrWhiteSpace(requesterEmail))
        {
            recipients.Add(requesterEmail!);
        }

        recipients.AddRange(await GetSubscriberEmailsAsync(ticket.Id));

        var subject = $"[Ticket #{ticket.Id}] Created - {ticket.Title}";
        var body = $"Ticket #{ticket.Id} was created. Status: {ticket.Status}.";
        await SendAsync(recipients, subject, body);
    }

    public async Task SendTicketAssignedAsync(Ticket ticket)
    {
        var recipients = new List<string>();
        if (!string.IsNullOrWhiteSpace(ticket.AssignedAdminUserId))
        {
            var assignedEmail = await GetUserEmailAsync(ticket.AssignedAdminUserId!);
            if (!string.IsNullOrWhiteSpace(assignedEmail))
            {
                recipients.Add(assignedEmail!);
            }
        }

        var requesterEmail = await GetUserEmailAsync(ticket.RequesterUserId);
        if (!string.IsNullOrWhiteSpace(requesterEmail))
        {
            recipients.Add(requesterEmail!);
        }

        recipients.AddRange(await GetSubscriberEmailsAsync(ticket.Id));

        var subject = $"[Ticket #{ticket.Id}] Assigned";
        var body = $"Ticket #{ticket.Id} was assigned. Status: {ticket.Status}.";
        await SendAsync(recipients, subject, body);
    }

    public async Task SendTicketStatusChangedAsync(Ticket ticket, TicketStatus oldStatus, TicketStatus newStatus)
    {
        var recipients = new List<string>();
        var requesterEmail = await GetUserEmailAsync(ticket.RequesterUserId);
        if (!string.IsNullOrWhiteSpace(requesterEmail))
        {
            recipients.Add(requesterEmail!);
        }

        if (!string.IsNullOrWhiteSpace(ticket.AssignedAdminUserId))
        {
            var assignedEmail = await GetUserEmailAsync(ticket.AssignedAdminUserId!);
            if (!string.IsNullOrWhiteSpace(assignedEmail))
            {
                recipients.Add(assignedEmail!);
            }
        }

        recipients.AddRange(await GetSubscriberEmailsAsync(ticket.Id));

        var subject = $"[Ticket #{ticket.Id}] Status Changed";
        var body = $"Ticket #{ticket.Id} status changed from {oldStatus} to {newStatus}.";
        await SendAsync(recipients, subject, body);
    }

    public async Task SendNewCommentAsync(Ticket ticket, TicketComment comment)
    {
        var author = await _userManager.FindByIdAsync(comment.AuthorUserId);
        var recipients = new List<string>();

        var isAdminAuthor = author != null && await _userManager.IsInRoleAsync(author, RoleNames.Admin);
        if (isAdminAuthor)
        {
            var requesterEmail = await GetUserEmailAsync(ticket.RequesterUserId);
            if (!string.IsNullOrWhiteSpace(requesterEmail))
            {
                recipients.Add(requesterEmail!);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(ticket.AssignedAdminUserId))
            {
                var assignedEmail = await GetUserEmailAsync(ticket.AssignedAdminUserId!);
                if (!string.IsNullOrWhiteSpace(assignedEmail))
                {
                    recipients.Add(assignedEmail!);
                }
            }

            if (!recipients.Any() && !string.IsNullOrWhiteSpace(_notificationOptions.AdminMailbox))
            {
                recipients.Add(_notificationOptions.AdminMailbox!);
            }
        }

        recipients.AddRange(await GetSubscriberEmailsAsync(ticket.Id));

        var subject = $"[Ticket #{ticket.Id}] New Comment";
        var body = $"A new comment was added to ticket #{ticket.Id}.";
        await SendAsync(recipients, subject, body);
    }

    private async Task<string?> GetUserEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.Email;
    }

    private async Task<List<string>> GetSubscriberEmailsAsync(int ticketId)
    {
        return await _db.TicketSubscribers
            .Where(s => s.TicketId == ticketId)
            .Join(_db.Users, s => s.UserId, u => u.Id, (_, user) => user.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .ToListAsync();
    }

    private async Task SendAsync(IEnumerable<string> recipients, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_smtpOptions.Host))
        {
            _logger.LogWarning("SMTP not configured. Email '{Subject}' not sent.", subject);
            return;
        }

        var recipientList = recipients.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (!recipientList.Any())
        {
            _logger.LogWarning("No email recipients for '{Subject}'.", subject);
            return;
        }

        using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = _smtpOptions.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_smtpOptions.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_smtpOptions.FromAddress, _smtpOptions.FromName),
            Subject = subject,
            Body = body
        };

        foreach (var recipient in recipientList)
        {
            message.To.Add(recipient);
        }

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email '{Subject}'.", subject);
        }
    }
}
