using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TicketingSystem.Helpers;

public static class TicketBodyRenderer
{
    private static readonly Regex ImageTokenRegex = new(@"\[\[image:(\d+)\]\]|(/attachments/view/(\d+))", RegexOptions.Compiled);

    public static string RenderTicketBody(string? bodyText, ISet<int> allowedAttachmentIds)
    {
        if (string.IsNullOrEmpty(bodyText))
        {
            return string.Empty;
        }

        var allowed = allowedAttachmentIds ?? new HashSet<int>();
        var builder = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in ImageTokenRegex.Matches(bodyText))
        {
            if (match.Index > lastIndex)
            {
                var text = bodyText.Substring(lastIndex, match.Index - lastIndex);
                builder.Append(EncodeWithLineBreaks(text));
            }

            var idText = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
            if (int.TryParse(idText, out var attachmentId) && allowed.Contains(attachmentId))
            {
                builder.Append($"<img src=\"/attachments/view/{attachmentId}\" alt=\"pasted image\" class=\"ticket-inline-image\" loading=\"lazy\" />");
            }
            else
            {
                builder.Append("<span class=\"ticket-inline-placeholder\">[image unavailable]</span>");
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < bodyText.Length)
        {
            builder.Append(EncodeWithLineBreaks(bodyText.Substring(lastIndex)));
        }

        return builder.ToString();
    }

    private static string EncodeWithLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return WebUtility.HtmlEncode(text)
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>");
    }
}
