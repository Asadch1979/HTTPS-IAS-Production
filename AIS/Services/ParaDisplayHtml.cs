using AngleSharp.Html.Parser;
using Ganss.Xss;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AIS.Services
{
    // Display only. Never use the result as editor input or for trusted workflow operations.
    public static class ParaDisplayHtml
    {
        private static bool HasMarkup(string value) => Regex.IsMatch(value, @"<\s*[a-zA-Z!][^>]*>");
        public static string Sanitize(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return string.Empty;
            // Some historical editor saves contain entity-encoded HTML. Decode only once,
            // and always pass the decoded result through the same sanitizer.
            var source = stored;
            if (!HasMarkup(source))
            {
                var decoded = WebUtility.HtmlDecode(source);
                if (HasMarkup(decoded)) source = decoded;
                else return "<p>" + WebUtility.HtmlEncode(source).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br>") + "</p>";
            }
            var document = new HtmlParser().ParseDocument(source);
            foreach (var node in document.QuerySelectorAll("script,style,iframe,object,embed,svg,math,template,form,input,button,textarea,select,link,meta,base,audio,video,source").ToArray())
                node.Remove();
            var sanitizer = new HtmlSanitizer(); // Per call: no mutable shared policy.
            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[] { "p", "div", "span", "br", "hr", "b", "strong", "i", "em", "u", "s", "sub", "sup", "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "blockquote", "pre", "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption" })
                sanitizer.AllowedTags.Add(tag);
            sanitizer.AllowedAttributes.Clear();
            foreach (var attribute in new[] { "colspan", "rowspan", "style" }) sanitizer.AllowedAttributes.Add(attribute);
            sanitizer.AllowedCssProperties.Clear();
            foreach (var property in new[] { "font-weight", "font-style", "text-decoration", "text-align", "vertical-align" }) sanitizer.AllowedCssProperties.Add(property);
            sanitizer.AllowedSchemes.Clear();
            sanitizer.UriAttributes.Clear(); // No URL-bearing attribute is allowed at all.
            sanitizer.KeepChildNodes = true; // Preserve text in harmless legacy formatting wrappers.
            foreach (var node in document.QuerySelectorAll("[colspan],[rowspan]"))
                foreach (var attribute in new[] { "colspan", "rowspan" })
                    if (node.HasAttribute(attribute) && (!int.TryParse(node.GetAttribute(attribute), out var count) || count < 1 || count > 100)) node.RemoveAttribute(attribute);
            return sanitizer.Sanitize(document.Body?.InnerHtml ?? string.Empty);
        }
    }
}
