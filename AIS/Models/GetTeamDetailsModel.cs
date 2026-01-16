using AIS.Validation;
namespace AIS.Models
    {
    public class GetTeamDetailsModel
        {
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string MEMBER_PPNO { get; set; }
        [PlainText]
        public string MEMBER_NAME { get; set; }
        [PlainText]
        public string ISTEAMLEAD { get; set; }
        [PlainText]
        public string AUDIT_START_DATE { get; set; }
        [PlainText]
        public string AUDIT_END_DATE { get; set; }




        }
    }
