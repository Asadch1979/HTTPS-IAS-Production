using System.Collections.Generic;

namespace AIS.Models
    {
    public class SaveObservationRequest
        {
        public List<ListObservationModel> LIST_OBS { get; set; }

        public int? ENG_ID { get; set; }
        public int? S_ID { get; set; }

        public int? V_CAT_ID { get; set; }
        public int? V_CAT_NATURE_ID { get; set; }
        public int? OTHER_ENTITY_ID { get; set; }
        public bool? IS_FINAL { get; set; }
        }

    }
