using System.ComponentModel.DataAnnotations;
using AIS.Validation;

namespace AIS.Models
    {
    public class AddAuditTeamModel
        {
        public int? ID { get; set; }

        [PlainText]
        public string T_CODE { get; set; }

        [PlainText]
        public string T_NAME { get; set; }

        public int? PPNO { get; set; }

        [PlainText]
        public string NAME { get; set; }

        [PlainText]
        public string PLACEOFPOSTING { get; set; }

        [PlainText]
        public string ISTEAMLEAD { get; set; }

        [PlainText]
        public string STATUS { get; set; }
        }
    }
