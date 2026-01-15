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
}
