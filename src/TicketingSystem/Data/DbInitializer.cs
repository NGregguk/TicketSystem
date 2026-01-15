using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingSystem.Models;
using TicketingSystem.Options;

namespace TicketingSystem.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedUserOptions>>().Value;

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
        }

        if (!await roleManager.RoleExistsAsync(RoleNames.Requester))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Requester));
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new Category { Name = "Hardware" },
                new Category { Name = "Software" },
                new Category { Name = "Access" },
                new Category { Name = "Network" },
                new Category { Name = "Other" });
            await db.SaveChangesAsync();
        }

        if (environment.IsDevelopment())
        {
            await EnsureUserAsync(userManager, seedOptions.AdminEmail, seedOptions.AdminPassword, RoleNames.Admin, "Admin User");
            await EnsureUserAsync(userManager, seedOptions.RequesterEmail, seedOptions.RequesterPassword, RoleNames.Requester, "Requester User");
        }
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role, string displayName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed user {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
