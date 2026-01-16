using AIS.Validation;
namespace AIS.Models
    {
    public class JoiningTeamModel
        {

        public int PP_NO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string IS_TEAM_LEAD { get; set; }


        }
    }
