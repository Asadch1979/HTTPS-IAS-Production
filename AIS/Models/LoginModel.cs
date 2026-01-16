using AIS.Validation;
namespace AIS.Models
    {
    public class LoginModel
        {
        [PlainText]
        public string PPNumber { get; set; }
        [PasswordText]
        public string Password { get; set; }
        }
    }
