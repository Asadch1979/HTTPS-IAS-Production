using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public class PendingLoginContextState
        {
        public int UserId { get; set; }
        public string PPNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string IsActive { get; set; }
        public string UserLocationType { get; set; }
        public string ChangePassword { get; set; }
        public bool PasswordChangeRequired { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<UserContextAssignmentModel> Contexts { get; set; } = new List<UserContextAssignmentModel>();
        }
    }
