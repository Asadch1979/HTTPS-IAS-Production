using AIS.Validation;
namespace AIS.Models
    {
    public class FADHOUserLegacyParaUserWiseParasPerformanceModel
        {
        [PlainText]
        public string PP_NO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string PARA_REVIEWED { get; set; }
        [PlainText]
        public string PARA_UPDATED { get; set; }
        [PlainText]
        public string PARA_UPDATED_WITHOUT_CHANGE { get; set; }
        [PlainText]
        public string PARA_REFERRED_BACK { get; set; }

        }
    }
