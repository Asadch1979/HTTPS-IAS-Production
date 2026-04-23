using System.Collections.Generic;

namespace AIS.Models
    {
    public class ContextSelectionViewModel
        {
        public string PPNumber { get; set; }
        public string UserName { get; set; }
        public int? SelectedAssignmentId { get; set; }
        public string ErrorMessage { get; set; }
        public IReadOnlyList<UserContextAssignmentModel> Contexts { get; set; } = new List<UserContextAssignmentModel>();
        }
    }
