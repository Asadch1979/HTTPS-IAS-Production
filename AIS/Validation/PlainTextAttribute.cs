using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AIS.Validation
{
    public class PlainTextAttribute : ValidationAttribute
    {
        private const string AllowedPattern = @"^[\p{L}\p{N}&,\?\p{P}\p{Zs}\r\n\t]*$";

        public PlainTextAttribute()
        {
            ErrorMessage = "Only letters, numbers, punctuation, and whitespace are allowed.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string text)
            {
                return new ValidationResult("Invalid text value.");
            }

            char[] forbiddenCharacters = { '<', '>', '=', '@', ':', '\\', '"' };
            if (text.IndexOfAny(forbiddenCharacters) >= 0)
            {
                return new ValidationResult("The characters <, >, =, @, :, \\, and \" are not allowed.");
            }

            if (!Regex.IsMatch(text, AllowedPattern))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
