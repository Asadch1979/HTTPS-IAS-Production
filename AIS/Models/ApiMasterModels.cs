using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class ApiMasterModel
        {
        public int ApiId { get; set; }
        public string ApiName { get; set; }
        public string ApiPath { get; set; }
        public string HttpMethod { get; set; }
        public string IsActive { get; set; }
        }


    public class ApiMasterSaveRequest
        {
        public int ApiId { get; set; }

        [Required]
        public string ApiName { get; set; }

        [Required]
        public string ApiPath { get; set; }

        [Required]
        public string HttpMethod { get; set; }

        [Required]
        public string IsActive { get; set; }

        [Required]
        public string ActionInd { get; set; }
        }
    }
