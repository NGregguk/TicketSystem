using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Data;
using TicketingSystem.Models;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Categories()
    {
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View("Categories", categories);
        }

        var category = new Category
        {
            Name = model.Name.Trim(),
            IsActive = model.IsActive
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Category added.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var viewModel = new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = await _db.Categories.FindAsync(model.Id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = model.Name.Trim();
        category.IsActive = model.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Category deactivated.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
        var viewModels = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            viewModels.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                Role = roles.FirstOrDefault() ?? RoleNames.Requester
            });
        }

        return View(viewModels);
    }

    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new EditUserViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        user.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();

        if (!string.Equals(user.UserName, model.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName.Trim());
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = "User updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = "User role updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var hasTickets = await _db.Tickets.AnyAsync(t => t.RequesterUserId == userId || t.AssignedAdminUserId == userId);
        var hasComments = await _db.TicketComments.AnyAsync(c => c.AuthorUserId == userId);
        var hasNotes = await _db.TicketInternalNotes.AnyAsync(n => n.AuthorUserId == userId);
        var hasAttachments = await _db.TicketAttachments.AnyAsync(a => a.UploadedByUserId == userId);

        if (hasTickets || hasComments || hasNotes || hasAttachments)
        {
            TempData["Error"] = "User cannot be deleted because they are linked to existing tickets, comments, notes, or attachments.";
            return RedirectToAction(nameof(Users));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, roles);
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            TempData["Error"] = $"Failed to delete user: {errors}";
            return RedirectToAction(nameof(Users));
        }

        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Users));
    }
}
