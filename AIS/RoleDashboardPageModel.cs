namespace AIS.Controllers
    {
    public class RoleDashboardPageModel
        {
        public int RoleId { get; set; }
        public int PageId { get; set; }
        public string? PageName { get; set; }
        public int DashboardOrder { get; set; }
        public string? Status { get; set; }
        
        }
    }