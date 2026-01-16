using AIS.Validation;
namespace AIS.Models
    {
    public class AvatarNameDisplayModel
        {
        [PlainText]
        public string PPNO { get; set; }
        public int Menu_Id { get; set; }
        [PlainText]
        public string Sub_Menu { get; set; }
        [PlainText]
        public string Sub_Menu_Id { get; set; }
        [PlainText]
        public string Sub_Menu_Name { get; set; }
        [PlainText]
        public string User_Entity_Name { get; set; }
        [PlainText]
        public string User_Role_Name { get; set; }
        public int Id { get; set; }
        [PlainText]
        public string Name { get; set; }

        }
    }
