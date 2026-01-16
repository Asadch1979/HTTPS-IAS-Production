using AIS.Validation;
namespace AIS.Models
    {
    public class EntityModel
        {
        public int EntityId { get; set; }
        [PlainText]
        public string EntityCode { get; set; }
        [PlainText]
        public string Name { get; set; }
        [PlainText]
        public string Type { get; set; }
        [PlainText]
        public string Allocatedto { get; set; }
        public int TotalParas { get; set; }

        }
    }
