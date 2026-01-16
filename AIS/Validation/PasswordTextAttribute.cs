using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AIS.Validation
{
    public class PasswordTextAttribute : ValidationAttribute
    {
        public PasswordTextAttribute()
        {
            ErrorMessage = "Password contains invalid characters.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string password)
            {
                return new ValidationResult("Invalid password value.");
            }

            if (password.Any(char.IsControl))
            {
                return new ValidationResult("Password cannot contain control characters.");
            }

            if (password.Contains('<') || password.Contains('>'))
            {
                return new ValidationResult("Password must not contain HTML or script content.");
            }

            return ValidationResult.Success;
        }
    }
}
