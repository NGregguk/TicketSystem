namespace TicketingSystem.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@local.test";
    public string FromName { get; set; } = "Ticketing System";
}
