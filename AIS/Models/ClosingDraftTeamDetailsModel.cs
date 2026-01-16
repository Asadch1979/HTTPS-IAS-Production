using AIS.Validation;
namespace AIS.Models
    {
    public class ClosingDraftTeamDetailsModel
        {

        public int ENG_PLAN_ID { get; set; }
        [PlainText]
        public string JOINING_DATE { get; set; }
        [PlainText]
        public string COMPLETION_DATE { get; set; }
        [PlainText]
        public string MEMBER_NAME { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        public int TEAM_MEM_PPNO { get; set; }
        public int ENTEREDBY { get; set; }
        public int TOTAL_NO_OB { get; set; }
        [PlainText]
        public string ISTEAMLEAD { get; set; }
        public int SUBMITTED_TO_AUDITEE { get; set; }
        public int RESPONDED { get; set; }
        public int DROPPED { get; set; }
        public int RESOLVED { get; set; }
        public int ADDED_TO_DRAFT { get; set; }

        }
    }
