using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public sealed class EntityShiftingHistoryModel
        {
        public int RefId { get; set; }
        public int OldEntityId { get; set; }
        public string OldEntityCode { get; set; } = string.Empty;
        public string OldEntity { get; set; } = string.Empty;
        public int NewEntityId { get; set; }
        public string NewEntityCode { get; set; } = string.Empty;
        public string NewEntity { get; set; } = string.Empty;
        public string CircularNo { get; set; }
        public DateTime? CircularDate { get; set; }
        public int? EnteredBy { get; set; }
        public DateTime? EnteredOn { get; set; }
        }

    public sealed class EntityShiftingParaModel
        {
        public string AuditPeriod { get; set; }
        public string ParaNo { get; set; }
        public string GistOfParas { get; set; }
        public string ParaStatus { get; set; } = string.Empty;
        public int? Annex { get; set; }
        }

    public sealed class EntityShiftingHistoryPageModel
        {
        public List<EntityShiftingHistoryModel> ShiftingRecords { get; set; } = new List<EntityShiftingHistoryModel>();
        }
    }
