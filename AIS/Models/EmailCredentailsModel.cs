using AIS.Validation;
namespace AIS.Models
    {
    public class EmailCredentailsModel
        {
        [RichTextSanitize]
        public string EMAIL { get; set; }
        [PasswordText]
        public string PASSWORD { get; set; }
        [PlainText]
        public string Host { get; set; }
        [PlainText]
        public string Username { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; } = true;
        public bool IsConfigured { get; set; }
        [PlainText]
        public string StatusMessage { get; set; }
        }
    }
