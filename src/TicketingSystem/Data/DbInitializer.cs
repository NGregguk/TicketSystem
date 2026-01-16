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
        var uploadOptions = scope.ServiceProvider.GetRequiredService<IOptions<UploadOptions>>().Value;

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

        if (!await db.InternalSystems.AnyAsync())
        {
            db.InternalSystems.AddRange(
                new InternalSystem { Name = "Helix" },
                new InternalSystem { Name = "Snapfulfil" },
                new InternalSystem { Name = "Aisle Print" },
                new InternalSystem { Name = "Barcode Scanning System" },
                new InternalSystem { Name = "Despatch Labels" }
            );
            await db.SaveChangesAsync();
        }

        if (environment.IsDevelopment())
        {
            await EnsureUserAsync(userManager, seedOptions.AdminEmail, seedOptions.AdminPassword, RoleNames.Admin, "Admin User");
            await EnsureUserAsync(userManager, seedOptions.RequesterEmail, seedOptions.RequesterPassword, RoleNames.Requester, "Requester User");
        }

        await CleanupTempAttachmentsAsync(db, environment, uploadOptions);
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

    private static async Task CleanupTempAttachmentsAsync(ApplicationDbContext db, IWebHostEnvironment environment, UploadOptions uploadOptions)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var tempAttachments = await db.TicketAttachments
            .Where(a => a.TicketId == null && a.TempKey != null && a.UploadedAtUtc < cutoff)
            .ToListAsync();

        if (!tempAttachments.Any())
        {
            return;
        }

        var root = Path.IsPathRooted(uploadOptions.RootPath)
            ? uploadOptions.RootPath
            : Path.Combine(environment.ContentRootPath, uploadOptions.RootPath);

        foreach (var attachment in tempAttachments)
        {
            var fullPath = Path.Combine(root, attachment.StoredFileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        db.TicketAttachments.RemoveRange(tempAttachments);
        await db.SaveChangesAsync();
    }
}
