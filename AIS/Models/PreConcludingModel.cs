using AIS.Validation;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class PreConcludingModel
        {
        [PlainText]
        public string OBS_ID { get; set; }

        [PlainText]
        [StringLength(50)]
        public string FINAL_PARA_NO { get; set; }

        [PlainText]
        [StringLength(500)]
        public string HEADING { get; set; }

        [PlainText]
        public string OBS_RISK { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string OBS_STATUS { get; set; }
        }
    }
