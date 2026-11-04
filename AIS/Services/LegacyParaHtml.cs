using Ganss.Xss;
namespace AIS.Services
{
    internal static class LegacyParaHtml
    {
        public static string Sanitize(string html)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[] { "p","div","span","br","hr","b","strong","i","em","u","s","strike","font","code","small","sub","sup","h1","h2","h3","h4","h5","h6","ul","ol","li","blockquote","pre","table","colgroup","col","thead","tbody","tfoot","tr","th","td","caption","a" }) sanitizer.AllowedTags.Add(tag);
            sanitizer.AllowedAttributes.Clear();
            foreach (var attribute in new[] { "style","colspan","rowspan","href","title","face","size","color","align","border","width","height","cellpadding","cellspacing","bgcolor" }) sanitizer.AllowedAttributes.Add(attribute);
            sanitizer.AllowedSchemes.Clear(); sanitizer.AllowedSchemes.Add("https"); sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedCssProperties.Clear();
            foreach (var css in new[] { "color","background-color","font-size","font-family","font-weight","font-style","text-decoration","text-align","vertical-align","border","border-collapse","padding","width" }) sanitizer.AllowedCssProperties.Add(css);
            return sanitizer.Sanitize(html ?? "");
        }
    }
}
