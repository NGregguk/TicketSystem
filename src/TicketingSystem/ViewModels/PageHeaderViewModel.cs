namespace TicketingSystem.ViewModels;

public class PageHeaderViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public Func<object?, Microsoft.AspNetCore.Html.IHtmlContent>? Actions { get; set; }
}
