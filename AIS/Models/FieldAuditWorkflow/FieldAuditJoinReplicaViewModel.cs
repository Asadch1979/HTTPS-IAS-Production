using System;
using AIS.Models;

namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditJoinReplicaViewModel
        {
        public int EngagementId { get; set; }
        public JoiningModel JoiningDetails { get; set; } = new JoiningModel();
        public DateTime CompletionDate { get; set; }
        public bool IsSubmitted { get; set; }
        public string CurrentMemberName { get; set; } = "User";
        public string MemberRoleLabel { get; set; } = "Team Member";
        public string EntityDisplayName { get; set; } = "-";
        public string SystemDateDisplay { get; set; } = DateTime.Now.ToString("dd-MM-yyyy");
        }
    }
