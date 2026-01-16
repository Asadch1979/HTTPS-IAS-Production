using AIS.Validation;
namespace AIS.Models
{
    public class PasswordResetResult
    {
        public bool AccountFound { get; set; }

        public bool PasswordReset { get; set; }
        [RichTextSanitize]
        public bool EmailSent { get; set; }

        [PlainText]
        public string Message { get; set; }
    }
}
