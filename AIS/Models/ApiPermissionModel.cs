using AIS.Validation;

namespace AIS.Models
    {
    public class ApiPermissionModel
        {
        [PlainText]
        public string ApiPath { get; set; }

        public string HttpMethod { get; set; }
        }
    }
