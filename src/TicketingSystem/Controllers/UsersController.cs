using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Data;
using TicketingSystem.Models;
using TicketingSystem.Services;

namespace TicketingSystem.Controllers;

[Authorize]
[Route("users")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly TicketAccessService _ticketAccess;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(ApplicationDbContext db, TicketAccessService ticketAccess, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _ticketAccess = ticketAccess;
        _userManager = userManager;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string? q, int? ticketId)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(Array.Empty<object>());
        }

        if (ticketId.HasValue)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            var userId = _ticketAccess.GetUserId(User) ?? string.Empty;
            var isAdmin = User.IsInRole(RoleNames.Admin);
            if (!_ticketAccess.CanManageSubscribers(userId, isAdmin, ticket))
            {
                return Forbid();
            }
        }

        var term = q.Trim();
        var search = _db.Users.AsQueryable();

        search = search.Where(u =>
            (u.DisplayName != null && EF.Functions.Like(u.DisplayName, $"%{term}%")) ||
            (u.Email != null && EF.Functions.Like(u.Email, $"%{term}%")) ||
            (u.UserName != null && EF.Functions.Like(u.UserName, $"%{term}%")));

        if (ticketId.HasValue)
        {
            var idValue = ticketId.Value;
            search = search.Where(u => !_db.TicketSubscribers.Any(s => s.TicketId == idValue && s.UserId == u.Id));
        }

        var users = await search
            .OrderBy(u => u.DisplayName ?? u.Email ?? u.UserName)
            .Take(10)
            .Select(u => new
            {
                userId = u.Id,
                displayName = u.DisplayName ?? u.Email ?? u.UserName ?? u.Id,
                email = u.Email ?? string.Empty
            })
            .ToListAsync();

        return Ok(users);
    }
}
