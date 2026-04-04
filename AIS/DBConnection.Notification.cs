using AIS.Models;
using AIS.Models.FieldAuditReport;
using AIS.Models.IID;
using AIS.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public bool IsObservationSubmittedToAuditee(int obsId)
            {
            if (obsId <= 0)
                {
                return false;
                }

            var observationText = GetManagedObservationTextForBranches(obsId).FirstOrDefault();
            var engId = observationText?.ENG_ID ?? 0;
            var observation = GetManagedObservationsForBranches(engId, obsId).FirstOrDefault(item => item.OBS_ID == obsId)
                ?? GetManagedObservationsForBranches(0, obsId).FirstOrDefault(item => item.OBS_ID == obsId);

            return observation != null
                && string.Equals((observation.OBS_STATUS ?? string.Empty).Trim(), "Submitted to Auditee", StringComparison.OrdinalIgnoreCase);
            }

        public ObservationSubmittedNotificationData GetObservationSubmittedNotificationData(int obsId)
            {
            var observationText = GetManagedObservationTextForBranches(obsId).FirstOrDefault();
            var engId = observationText?.ENG_ID ?? 0;
            var observation = GetManagedObservationsForBranches(engId, obsId).FirstOrDefault(item => item.OBS_ID == obsId)
                ?? GetManagedObservationsForBranches(0, obsId).FirstOrDefault(item => item.OBS_ID == obsId);
            var entityId = ParseNullableInt(observation?.ENTITY_ID);

            return new ObservationSubmittedNotificationData
                {
                EngagementId = engId > 0 ? engId : (int?)null,
                ObservationId = obsId,
                ObservationReference = observationText?.ReferenceId?.ToString(CultureInfo.InvariantCulture) ?? obsId.ToString(CultureInfo.InvariantCulture),
                EntityName = observation?.ENTITY_NAME ?? string.Empty,
                AuditPeriod = observation?.PERIOD ?? string.Empty,
                ObservationHeading = observationText?.HEADING ?? observation?.HEADING ?? string.Empty,
                ObservationStatus = observation?.OBS_STATUS ?? string.Empty,
                ObservationSummary = observationText?.OBS_TEXT ?? string.Empty,
                ToRecipients = GetAuditeeRecipientEmails(entityId)
                };
            }

        public AuditTaskAssignedNotificationData GetAuditTaskAssignedNotificationData(AuditEngagementPlanModel plan)
            {
            plan ??= new AuditEngagementPlanModel();
            var tentativePlan = GetTentativePlansForFields()
                .FirstOrDefault(item => plan.PLAN_ID.HasValue && item.PLAN_ID == plan.PLAN_ID.Value);
            var teamRows = GetAuditTeams()
                .Where(item => plan.TEAM_ID.HasValue && item.T_ID == plan.TEAM_ID.Value)
                .ToList();
            var teamMemberIds = teamRows
                .Select(item => item.TEAMMEMBER_ID)
                .Distinct()
                .ToList();

            return new AuditTaskAssignedNotificationData
                {
                PlanId = plan.PLAN_ID,
                EngagementId = plan.ENG_ID,
                EngagementLabel = BuildEngagementLabel(plan, tentativePlan),
                EntityName = ChooseFirstNonEmpty(plan.ENTITY_NAME, tentativePlan?.ENTITY_NAME),
                AuditPeriod = tentativePlan?.PERIOD_NAME ?? string.Empty,
                ReportingOffice = tentativePlan?.REPORTING_OFFICE ?? string.Empty,
                TeamName = plan.TEAM_NAME ?? string.Empty,
                AuditStartDate = plan.AUDIT_STARTDATE,
                AuditEndDate = plan.AUDIT_ENDDATE,
                OperationalStartDate = plan.OP_STARTDATE,
                OperationalEndDate = plan.OP_ENDDATE,
                TeamMembers = teamRows.Select(item => item.EMPLOYEENAME).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                ToRecipients = GetUserEmailsByPpNumbers(teamMemberIds)
                };
            }

        public InquiryAssignedNotificationData GetInquiryAssignedNotificationData(HeadReviewModel model)
            {
            model ??= new HeadReviewModel();
            var complaintId = model.ComplaintId.GetValueOrDefault();
            var complaint = complaintId > 0 ? GetComplaint(complaintId) : null;
            var unit = GetInspectionUnits().FirstOrDefault(item => item.I_ID == model.AssignedToUnit);

            return new InquiryAssignedNotificationData
                {
                ComplaintId = complaintId,
                InquiryReference = ChooseFirstNonEmpty(complaint?.ComplaintNo, complaintId > 0 ? $"INQ-{complaintId}" : string.Empty),
                InquiryNature = complaint?.Nature ?? string.Empty,
                AssignedUnitName = ChooseFirstNonEmpty(unit?.UNIT_NAME, complaint?.AssignedUnit),
                Directions = model.Directions ?? string.Empty,
                AssignedOn = model.AssignedOn ?? string.Empty,
                DueDate = model.DueDate ?? string.Empty,
                ToRecipients = GetUsersByEntityId(model.AssignedToUnit)
                };
            }

        public FinalReportIssuedNotificationData GetFinalReportIssuedNotificationData(int engId)
            {
            var overview = GetFieldAuditReportOverview(engId);
            return new FinalReportIssuedNotificationData
                {
                EngagementId = engId > 0 ? engId : (int?)null,
                EngagementLabel = BuildFinalReportLabel(overview),
                EntityName = overview?.EntityName ?? overview?.ENTITY_NAME ?? string.Empty,
                AuditPeriod = overview?.AuditPeriod ?? string.Empty,
                ReportingOffice = overview?.REPORTING_OFFICE ?? string.Empty,
                TeamName = overview?.TeamName ?? string.Empty,
                ReportVersion = overview?.VersionNo ?? string.Empty,
                FinalizedOn = overview?.FinalizedOn,
                ToRecipients = GetAuditeeRecipientEmails(overview?.EntityId)
                };
            }

        private List<string> GetAuditeeRecipientEmails(int? entityId)
            {
            var recipients = new List<string>();
            if (!entityId.HasValue || entityId.Value <= 0)
                {
                return recipients;
                }

            var auditeeEmails = GetAuditeeEntitiesForUpdate(0, entityId.Value)
                .Select(item => item.EMAIL_ADDRESS)
                .Where(item => !string.IsNullOrWhiteSpace(item));
            recipients.AddRange(auditeeEmails);
            recipients.AddRange(GetUsersByEntityId(entityId.Value));
            return DistinctEmails(recipients);
            }

        private List<string> GetUsersByEntityId(int entityId)
            {
            if (entityId <= 0)
                {
                return new List<string>();
                }

            return DistinctEmails(GetAllUsers(new FindUserModel { ENTITYID = entityId }).Select(item => item.Email));
            }

        private List<string> GetUserEmailsByPpNumbers(IEnumerable<int> ppNumbers)
            {
            var recipients = new List<string>();
            foreach (var ppNumber in ppNumbers.Distinct())
                {
                var user = GetAllUsers(new FindUserModel { PPNUMBER = ppNumber }).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(user?.Email))
                    {
                    recipients.Add(user.Email);
                    }
                }

            return DistinctEmails(recipients);
            }

        private static List<string> DistinctEmails(IEnumerable<string> emails)
            {
            return (emails ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            }

        private static int? ParseNullableInt(string value)
            {
            return int.TryParse(value, out var parsed) ? parsed : (int?)null;
            }

        private static string BuildEngagementLabel(AuditEngagementPlanModel plan, TentativePlanModel tentativePlan)
            {
            var entityName = ChooseFirstNonEmpty(plan?.ENTITY_NAME, tentativePlan?.ENTITY_NAME);
            var auditPeriod = tentativePlan?.PERIOD_NAME ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(entityName) && !string.IsNullOrWhiteSpace(auditPeriod))
                {
                return $"{entityName} - {auditPeriod}";
                }

            return ChooseFirstNonEmpty(entityName, auditPeriod, plan?.PLAN_ID.HasValue == true ? $"Plan {plan.PLAN_ID.Value}" : string.Empty);
            }

        private static string BuildFinalReportLabel(FieldAuditReportOverviewModel overview)
            {
            if (overview == null)
                {
                return string.Empty;
                }

            var entityName = ChooseFirstNonEmpty(overview.EntityName, overview.ENTITY_NAME);
            if (!string.IsNullOrWhiteSpace(entityName) && !string.IsNullOrWhiteSpace(overview.AuditPeriod))
                {
                return $"{entityName} - {overview.AuditPeriod}";
                }

            return ChooseFirstNonEmpty(entityName, overview.AuditPeriod, overview.REPORTING_OFFICE);
            }

        private static string ChooseFirstNonEmpty(params string[] values)
            {
            return values?.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item))?.Trim() ?? string.Empty;
            }
        }
    }
