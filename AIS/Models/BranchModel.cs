using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class BranchModel
        {
        public int BRANCHID { get; set; }
        public int ZONEID { get; set; }
        [PlainText]
        public string BRANCHNAME { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }
        [PlainText]
        public string BRANCHCODE { get; set; }
        public int BRANCH_SIZE_ID { get; set; }
        [NotMapped]
        [PlainText]
        public string BRANCH_SIZE { get; set; }
        [NotMapped]
        [PlainText]
        public string ZONE_NAME { get; set; }
        }
    }
