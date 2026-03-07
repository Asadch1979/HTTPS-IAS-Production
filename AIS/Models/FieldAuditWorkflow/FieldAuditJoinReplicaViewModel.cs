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
        }
    }
