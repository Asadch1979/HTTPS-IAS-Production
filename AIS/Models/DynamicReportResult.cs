using System.Collections.Generic;

namespace AIS.Models.SM
    {
    public class DynamicReportResult
        {
        public List<ExceptionReportFormatModel> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }

        public DynamicReportResult()
            {
            Columns = new List<ExceptionReportFormatModel>();
            Rows = new List<Dictionary<string, object>>();
            }
        }
    }