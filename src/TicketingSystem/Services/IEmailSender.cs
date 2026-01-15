using TicketingSystem.Models;

namespace TicketingSystem.Services;

public interface IEmailSender
{
    Task SendTicketCreatedAsync(Ticket ticket);
    Task SendTicketAssignedAsync(Ticket ticket);
    Task SendTicketStatusChangedAsync(Ticket ticket, TicketStatus oldStatus, TicketStatus newStatus);
    Task SendNewCommentAsync(Ticket ticket, TicketComment comment);
}
