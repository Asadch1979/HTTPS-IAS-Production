using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace AIS.Utilities;

public static class MemoHtmlHelper
{
    private static readonly Regex ParagraphSplitRegex = new("\n{2,}", RegexOptions.Compiled);

    public static IHtmlContent FormatMemoAsHtml(string? memo)
    {
        if (string.IsNullOrWhiteSpace(memo))
        {
            return HtmlString.Empty;
        }

        var normalized = memo.Replace("\r\n", "\n").Replace("\r", "\n");
        var paragraphs = ParagraphSplitRegex.Split(normalized);
        var builder = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim('\n');
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var encoded = HtmlEncoder.Default.Encode(trimmed);
            var withBreaks = encoded.Replace("\n", "<br/>");

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append("<p>")
                .Append(withBreaks)
                .Append("</p>");
        }

        return new HtmlString(builder.ToString());
    }
}
