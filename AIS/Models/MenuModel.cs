using AIS.Validation;
namespace AIS.Models
    {
    public class MenuModel
        {
        public int Menu_Id { get; set; }
        [PlainText]
        public string Menu_Name { get; set; }
        [PlainText]
        public string Menu_Order { get; set; }
        [PlainText]
        public string Menu_Description { get; set; }
        }
    }
