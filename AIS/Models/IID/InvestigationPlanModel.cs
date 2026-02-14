using System;
using System.Collections.Generic;
using AIS.Validation;

namespace AIS.Models.IID
{
    public class InvestigationPlanModel
    {
        // Existing/basic
        public int PlanId { get; set; }                 // PLAN_ID
        public int ComplaintId { get; set; }            // COMPLAINT_ID

        [PlainText]
        public string PlanDetails { get; set; }         // PLAN_DETAILS (CLOB)

        public int SubmittedBy { get; set; }            // SUBMITTED_BY
        public DateTime? SubmittedOn { get; set; }      // SUBMITTED_ON

        [PlainText]
        public string Status { get; set; }              // STATUS

        [PlainText]
        public string PlanTitle { get; set; }           // PLAN_TITLE

        // New structured fields (Option-A columns)
        [PlainText]
        public string InvestigationRisk { get; set; }   // INVESTIGATION_RISK  ('High'/'Medium'/'Low')

        [PlainText]
        public string InvestigationSize { get; set; }   // INVESTIGATION_SIZE  ('Small'/'Medium'/'Large')

        public int? NoOfDays { get; set; }              // NO_OF_DAYS
        public int? TravellingDays { get; set; }        // TRAVELLING_DAYS

        public DateTime? StartDate { get; set; }        // START_DATE

        [PlainText]
        public string TeamLead { get; set; }            // TEAM_LEAD

        [PlainText]
        public string TeamMembers { get; set; }         // TEAM_MEMBERS

        [PlainText]
        public string ActivitiesText { get; set; }      // ACTIVITIES_TEXT (if added)

        public Dictionary<string, string> AdditionalFields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
