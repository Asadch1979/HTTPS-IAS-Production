using System.Collections.Generic;

namespace AIS.Models
    {
    namespace AIS.Models.Execution   // adjust to your project structure
        {
        public class SaveObservationCauRequest
            {
            public List<ListObservationModel> LIST_OBS { get; set; }

            public int? ENG_ID { get; set; }
            public int? BRANCH_ID { get; set; }
            public int? SUB_CHECKLISTID { get; set; }
            public int? CHECKLIST_ID { get; set; }
            public string ANNEXURE_ID { get; set; }
            public bool? IS_FINAL { get; set; }
            }
        }


    }
