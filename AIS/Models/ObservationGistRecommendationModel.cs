using AIS.Validation;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ObservationGistRecommendationModel
    {
        public int OBS_ID { get; set; }

        [RichTextSanitize]
        [StringLength(4000)]
        public string GIST_OF_PARA { get; set; }

        [RichTextSanitize]
        [StringLength(4000)]
        public string AUDITOR_RECOMMENDATION { get; set; }
    }
}
