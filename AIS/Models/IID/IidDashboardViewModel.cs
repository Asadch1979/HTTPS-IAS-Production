using System.Collections.Generic;

namespace AIS.Models.IID
    {
    public class IidDashboardViewModel
        {
        public string Title { get; set; } = "IID Case Dashboard";
        public int DashboardPageId { get; set; }
        public int ComplaintDropdownPageId { get; set; }
        public string CurrentItemKey { get; set; }
        public int SelectedComplaintId { get; set; }
        public List<IidDashboardItemModel> Steps { get; set; } = new List<IidDashboardItemModel>();
        public List<IidDashboardItemModel> Utilities { get; set; } = new List<IidDashboardItemModel>();
        }
    }
