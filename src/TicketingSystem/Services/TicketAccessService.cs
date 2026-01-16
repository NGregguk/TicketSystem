using Microsoft.AspNetCore.Identity;
using TicketingSystem.Models;

namespace TicketingSystem.Services;

public class TicketAccessService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketAccessService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IQueryable<Ticket> ApplyViewScope(IQueryable<Ticket> query, string userId, bool isAdmin)
    {
        return query;
    }

    public async Task<bool> CanViewTicketAsync(string userId, bool isAdmin, Ticket ticket)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> CanViewTicketAsync(string userId, bool isAdmin, int ticketId)
    {
        return await Task.FromResult(true);
    }

    public bool CanManageSubscribers(string userId, bool isAdmin, Ticket ticket)
    {
        if (isAdmin)
        {
            return true;
        }

        return ticket.RequesterUserId == userId;
    }

    public string? GetUserId(System.Security.Claims.ClaimsPrincipal user)
    {
        return _userManager.GetUserId(user);
    }
}
