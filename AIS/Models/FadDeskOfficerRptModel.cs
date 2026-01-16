using AIS.Validation;

namespace AIS.Controllers
    {
    public class FadDeskOfficerRptModel
        {
        [PlainText]
        public string AuditPeriod { get; set; }
        [PlainText]
        public string ChildCode { get; set; }
        [PlainText]
        public string CName { get; set; }
        [PlainText]
        public string AZ { get; set; }
        [PlainText]
        public string PName { get; set; }
        [PlainText]
        public string Annex { get; set; }
        [PlainText]
        public string GistOfParas { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        public int NoOfInstances { get; set; }
        [PlainText]
        public string Risk { get; set; }
        [PlainText]
        public string Amount { get; set; }
        [PlainText]
        public string Status { get; set; }

        }
    }
