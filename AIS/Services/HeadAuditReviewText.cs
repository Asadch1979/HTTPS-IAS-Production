using AngleSharp.Html.Parser;
using System.Linq;

namespace AIS.Services
{
    // Saved editor content is presented as plain, Razor-encoded text in the
    // supervisory reader. Parsing does not execute scripts or fetch resources.
    public static class HeadAuditReviewText
    {
        public static string Plain(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var document = new HtmlParser().ParseDocument(value);
            foreach (var node in document.QuerySelectorAll("script,style,iframe,object").ToArray()) node.Remove();
            foreach (var node in document.QuerySelectorAll("br,p,div,li,tr").ToArray())
                node.AppendChild(document.CreateTextNode("\n"));
            return document.Body?.TextContent.Trim();
        }
    }
}
