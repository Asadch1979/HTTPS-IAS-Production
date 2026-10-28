using AIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        private void ValidateDatedDashboardScope(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            if (ManagementDashboardPeriod.Applies(scope.DashboardType))
            {
                ValidateExecutiveDashboardSession(scope);
                RequireExecutiveGroupScope(scope);
                if (scope.EntityId.GetValueOrDefault() <= 0) throw new UnauthorizedAccessException();
                ManagementDashboardPeriod.Apply(filters, scope.DashboardType);
                ComplianceDashboardPolicy.Validate(filters);
                return;
            }
            if (scope.DashboardType != "CCO")
            {
                ValidateSnapScope(scope);
                AuditParaSnapPolicy.Validate(filters);
                return;
            }
            var user = ValidateExecutiveDashboardSession(scope);
            if (scope.PermissionPagePath != "/ExecutiveOverview/CCO" ||
                (user.UserRoleID != 38 && !(user.UserRoleID == 1 && CreateSessionHandler().IsSuperUser())))
                throw new UnauthorizedAccessException();
            ComplianceDashboardPolicy.Validate(filters);
        }

        private List<AuditParaSnapBucket> ComplianceDistribution(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => GetAuditParaSnapDistribution(scope, filters);

        private ExecutiveKpiSummary ComplianceKpis(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateDatedDashboardScope(scope, filters);
            var k = ReadExecutiveKpiLifecycle(scope, filters);
            var buckets = ComplianceDistribution(scope, filters);
            long Required(long? value) => value ?? throw new InvalidOperationException("Dated KPI unavailable.");
            var opening = Required(k.OpeningOpenParas);
            var added = Required(k.ParaAdded);
            var settled = Required(k.ParasSettled);
            var total = opening + added;
            var rate = total > 0 ? Math.Round(100m * settled / total, 2) : (decimal?)null;
            ValidateDatedDashboardScope(scope, filters);
            return new ExecutiveKpiSummary {
                OpeningCases = scope.DashboardType == "CCO" ? opening : (long?)null,
                ParaAdded = scope.DashboardType == "CCO" ? added : (long?)null,
                TotalCases = total, OpenParas = Required(k.ClosingOpenParas),
                HighRiskParas = Required(k.HighRiskOpenParas), CriticalItems = Required(k.HighRiskOpenParas),
                Over1YearParas = Required(k.Over1YearParas),
                ZeroCycleParas = buckets.Where(b => b.Dataset == "cycle" && b.BucketCode == "0").Sum(b => Required(b.ParaCount)),
                Over3CycleParas = buckets.Where(b => b.Dataset == "cycle" && b.BucketCode == "4").Sum(b => Required(b.ParaCount)),
                SettledTotal = settled, SettledMtd = settled,
                SettlementRatio = rate ?? 0, SettlementMtdRate = rate
            };
        }
        private List<ExecutiveRiskSummary> ComplianceRisk(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            var rows = ComplianceDistribution(scope, filters).Where(b => b.Dataset == "risk").ToList();
            var total = rows.Sum(b => b.ParaCount ?? 0);
            return rows.Select(b => new ExecutiveRiskSummary { RiskId = int.Parse(b.BucketCode),
                RiskName = b.BucketName, ParaCount = b.ParaCount ?? 0,
                Percentage = total > 0 ? Math.Round(100m * (b.ParaCount ?? 0) / total, 2) : 0 }).ToList();
        }
        private List<ExecutiveCriticalItem> ComplianceCritical(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters, int maxRows)
            => GetAuditParaSnapCritical(scope, filters, Math.Min(maxRows, 250)).Select(b => new ExecutiveCriticalItem {
                ComId = b.ComId, EntityId = b.EntityId, EntityName = b.EntityName, ParaNo = b.ParaNo,
                GistOfParas = b.GistOfParas, RiskId = 1, RiskName = b.RiskName, AgingDays = b.AgingDays,
                ComCycle = b.ComCycle, AuditPeriod = b.AuditPeriod, ParaAddedOn = b.ParaAddedOn,
                DaysSinceLastMovement = b.DaysSinceLastMovement }).ToList();
    }
}
