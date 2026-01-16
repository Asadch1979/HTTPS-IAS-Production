using System.Collections.Generic;

namespace AIS.Models
    {
    public class ValidationErrorResponse
        {
        public bool Status { get; set; }
        public string Message { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }
        }
    }
