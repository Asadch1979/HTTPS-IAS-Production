using System;

namespace AIS.Models.IID
    {
    public class IidDashboardItemModel
        {
        public int SequenceNo { get; set; }
        public string ItemKey { get; set; }
        public string Title { get; set; }
        public string SourcePath { get; set; }
        public int RequiredPermissionPageId { get; set; }
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }
        public bool RequiresComplaintSelection { get; set; }
        public bool ReloadOnComplaintChange { get; set; }
        public string DisabledMessage { get; set; }
        public bool IsStep { get; set; }

        public string QueryKey => IsStep ? "stepCode" : "utilityCode";
        public string ItemType => IsStep ? "step" : "utility";
        }
    }
