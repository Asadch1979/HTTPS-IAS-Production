using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIS.Validation
{
    public class EmailTextAttribute : ValidationAttribute
    {
        private static readonly Regex AllowedPattern = new(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        public EmailTextAttribute()
        {
            ErrorMessage = "Email contains invalid characters.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string email)
            {
                return new ValidationResult("Invalid email value.");
            }

            if (email.Any(char.IsControl))
            {
                return new ValidationResult("Email cannot contain control characters.");
            }

            if (email.Contains('<') || email.Contains('>'))
            {
                return new ValidationResult("Email must not contain HTML or script content.");
            }

            if (!AllowedPattern.IsMatch(email))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
