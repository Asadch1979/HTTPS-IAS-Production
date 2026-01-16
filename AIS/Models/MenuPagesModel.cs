using AIS.Validation;
namespace AIS.Models
    {
    public class MenuPagesModel
        {
        public int Id { get; set; }
        public int Menu_Id { get; set; }
        public int PageId { get; set; }
        [PlainText]
        public string Page_Name { get; set; }
        [PlainText]
        public string Page_Key { get; set; }

        [PlainText]
        public string Page_URL { get; set; }

        [PlainText]
        public string Page_Path { get; set; }
        public int Page_Order { get; set; }
        [PlainText]
        public string Status { get; set; }
        [PlainText]
        public string Sub_Menu { get; set; }
        [PlainText]
        public string Sub_Menu_Id { get; set; }
        [PlainText]
        public string Sub_Menu_Name { get; set; }
        public int Hide_Menu { get; set; }
        }
    }
