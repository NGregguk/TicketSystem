using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TicketingSystem.Options;

namespace TicketingSystem.Services;

public class FileSystemStorage : IFileStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly UploadOptions _options;
    private readonly ILogger<FileSystemStorage> _logger;

    public FileSystemStorage(IWebHostEnvironment environment, IOptions<UploadOptions> options, ILogger<FileSystemStorage> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsAllowed(IFormFile file, out string? error)
    {
        error = null;

        if (file.Length <= 0)
        {
            error = "File is empty.";
            return false;
        }

        if (file.Length > _options.MaxSizeBytes)
        {
            error = $"File exceeds {_options.MaxSizeBytes / (1024 * 1024)} MB limit.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(ext))
        {
            error = "File type is not allowed.";
            return false;
        }

        return true;
    }

    public Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        return SaveAsync(file, null, cancellationToken);
    }

    public async Task<string> SaveAsync(IFormFile file, string? subfolder, CancellationToken cancellationToken = default)
    {
        var root = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(_environment.ContentRootPath, _options.RootPath);

        var targetRoot = string.IsNullOrWhiteSpace(subfolder)
            ? root
            : Path.Combine(root, subfolder);

        Directory.CreateDirectory(targetRoot);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var relativePath = string.IsNullOrWhiteSpace(subfolder)
            ? storedFileName
            : Path.Combine(subfolder, storedFileName);
        var fullPath = Path.Combine(targetRoot, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation("Stored attachment {StoredFileName} ({SizeBytes} bytes)", relativePath, file.Length);
        return relativePath;
    }
}
