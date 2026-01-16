using AIS.Validation;
namespace AIS.Models
    {
    public class ZoneBranchParaStatusModel
        {
        [PlainText]
        public string Entity_Name { get; set; }
        public int Total_Paras { get; set; }
        public int Settled_Paras { get; set; }
        public int Unsettled_Paras { get; set; }

        }
    }
