namespace TicketingSystem.Options;

public class UploadOptions
{
    public const string SectionName = "Uploads";

    public string RootPath { get; set; } = "App_Data/Uploads";
    public long MaxSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".txt", ".docx", ".xlsx" };
}
