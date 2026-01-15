using Microsoft.AspNetCore.Http;

namespace TicketingSystem.Services;

public interface IFileStorage
{
    bool IsAllowed(IFormFile file, out string? error);
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
}
