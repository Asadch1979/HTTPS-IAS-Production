using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ganss.Xss;


namespace AIS.Validation
{
    public class RichTextSanitizeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string html)
            {
                return new ValidationResult("Invalid rich text value.");
            }

            var sanitizer = CreateSanitizer();
            var sanitized = sanitizer.Sanitize(html);

            if (validationContext.MemberName != null)
            {
                PropertyInfo? property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                if (property != null && property.CanWrite && property.PropertyType == typeof(string))
                {
                    property.SetValue(validationContext.ObjectInstance, sanitized);
                }
            }

            return ValidationResult.Success;
        }

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.UnionWith(new[]
            {
                "a", "abbr", "b", "blockquote", "br", "caption", "cite", "code", "col", "colgroup",
                "div", "em", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "i", "img", "li", "ol",
                "p", "pre", "q", "small", "span", "strong", "sub", "sup", "table", "tbody", "td",
                "tfoot", "th", "thead", "tr", "u", "ul"
            });

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.UnionWith(new[]
            {
                "href", "title", "alt", "src", "class", "colspan", "rowspan", "target", "rel"
            });

            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedCssProperties.UnionWith(new[]
            {
                "color", "background-color", "font-size", "font-weight", "text-align", "border", "border-collapse",
                "padding", "margin"
            });

            sanitizer.AllowedClasses.Clear();

            return sanitizer;
        }
    }
}
