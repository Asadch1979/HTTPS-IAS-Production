using AIS.Validation;
namespace AIS.Models
    {
    public class StatusWiseComplianceModel
        {
        [PlainText]
        public string parent_id { get; set; }
        [PlainText]
        public string parent_Office { get; set; }
        [PlainText]
        public string entity_id { get; set; }
        [PlainText]
        public string entity_name { get; set; }
        [PlainText]
        public string auditby_id { get; set; }
        [PlainText]
        public string complaince_Submitted { get; set; }
        [PlainText]
        public string complaince_received_at_Incharge_implementation { get; set; }
        [PlainText]
        public string referredback_by_Controlling_office { get; set; }
        [PlainText]
        public string complaince_Submitted_To_Incharge_AZ { get; set; }
        [PlainText]
        public string complaince_Referred_back_by_Incharge_Implementation { get; set; }
        [PlainText]
        public string para_settled_by_Incharge_AZ { get; set; }
        [PlainText]
        public string complaince_Referred_back_by_Incharge_AZ { get; set; }


        }
    }
