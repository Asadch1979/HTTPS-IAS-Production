using System.Collections.Generic;

namespace AIS.Models
    {
    public class SaveParaReferencesRequestModel
        {
        public int? ComId { get; set; }
        public List<ParaReferenceLinkModel> References { get; set; }
        }
    }
