using AIS.Validation;
namespace AIS.Models
    {
    public class IdNameModel
        {
        public int Id { get; set; }
        [PlainText]
        public string Name { get; set; }
        }
    }
