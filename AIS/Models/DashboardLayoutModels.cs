using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class DashboardLayoutPageModel
        {
        public int RoleId { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PageUrl { get; set; }
        public string PagePath { get; set; }
        public int PageOrder { get; set; }
        public int DashboardOrder { get; set; }
        public string IsActive { get; set; }
        }

    public class DashboardLayoutSaveRequest
        {
        public int RoleId { get; set; }
        public int PageId { get; set; }
        public int DashboardOrder { get; set; }
        public string IsActive { get; set; }
        public string ActionInd { get; set; }
        }
    }
