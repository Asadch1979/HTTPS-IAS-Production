using AIS.Validation;
namespace AIS.Models
    {
    public class UpdateUserModel
        {

        public int USER_ID { get; set; }
        public int ROLE_ID { get; set; }
        public int ENTITY_ID { get; set; }
        [PasswordText]
        public string PASSWORD { get; set; }
        [RichTextSanitize]
        public string EMAIL_ADDRESS { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }

        }
    }
