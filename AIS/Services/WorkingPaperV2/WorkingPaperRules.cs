using AIS.Models.WorkingPaperV2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace AIS.Services.WorkingPaperV2
    {
    public static class WorkingPaperRules
        {
        private static readonly IReadOnlyDictionary<string, string[]> RequiredFields =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [WorkingPaperTypes.LoanCase] = new[] { "loanNumber", "productType", "currencyCode", "outstandingAmount", "classification", "overdueDays" },
                [WorkingPaperTypes.Voucher] = new[] { "voucherNumber", "postingDate", "voucherType", "currencyCode", "amount", "debitAccount", "creditAccount" },
                [WorkingPaperTypes.AccountOpening] = new[] { "accountNumber", "openDate", "customerType", "accountType", "riskRating", "screeningResult" },
                [WorkingPaperTypes.FixedAsset] = new[] { "assetTag", "description", "assetClass", "recordedLocation", "exists", "recordedNetBookValue" },
                [WorkingPaperTypes.CashCount] = new[] { "currencyCode", "denomination", "physicalQuantity", "registerQuantity", "ledgerBalance" }
            };

        public static IReadOnlyList<string> ValidateItem(string paperType, JsonElement details, string result)
            {
            var errors = new List<string>();
            if (!WorkingPaperTypes.All.Contains(paperType))
                {
                errors.Add("Unsupported working-paper type.");
                return errors;
                }
            if (details.ValueKind != JsonValueKind.Object)
                {
                errors.Add("Item details must be a JSON object.");
                return errors;
                }
            foreach (var field in RequiredFields[paperType])
                {
                if (!details.TryGetProperty(field, out var value) || IsEmpty(value))
                    errors.Add($"{field} is required.");
                }
            ValidateNumbers(paperType, details, errors);
            if (string.Equals(result, "EXCEPTION", StringComparison.OrdinalIgnoreCase)
                && (!details.TryGetProperty("exceptionSummary", out var summary) || IsEmpty(summary)))
                errors.Add("exceptionSummary is required when the result is Exception.");
            return errors;
            }

        public static Dictionary<string, decimal> Calculate(string paperType, JsonElement details)
            {
            var values = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (paperType == WorkingPaperTypes.CashCount)
                {
                var denomination = Decimal(details, "denomination");
                var physicalQuantity = Decimal(details, "physicalQuantity");
                var registerQuantity = Decimal(details, "registerQuantity");
                values["physicalAmount"] = denomination * physicalQuantity;
                values["registerAmount"] = denomination * registerQuantity;
                values["quantityVariance"] = physicalQuantity - registerQuantity;
                values["amountVariance"] = values["physicalAmount"] - values["registerAmount"];
                }
            else if (paperType == WorkingPaperTypes.LoanCase)
                {
                values["provisionVariance"] = Decimal(details, "expectedProvision") - Decimal(details, "recordedProvision");
                values["securityShortfall"] = Math.Max(0, Decimal(details, "requiredSecurityValue") - Decimal(details, "eligibleSecurityValue"));
                }
            else if (paperType == WorkingPaperTypes.FixedAsset)
                values["valuationVariance"] = Decimal(details, "expectedNetBookValue") - Decimal(details, "recordedNetBookValue");
            else if (paperType == WorkingPaperTypes.Voucher)
                values["amountVariance"] = Decimal(details, "recalculatedAmount") - Decimal(details, "amount");
            return values;
            }

        public static IReadOnlyList<string> ValidatePlan(WorkingPaperPlanModel plan)
            {
            var errors = new List<string>();
            if (plan == null) return new[] { "Plan is required." };
            if (string.IsNullOrWhiteSpace(plan.Objectives)) errors.Add("Audit objectives are required.");
            if (string.IsNullOrWhiteSpace(plan.Risks)) errors.Add("Risks are required.");
            if (string.IsNullOrWhiteSpace(plan.Controls)) errors.Add("Expected controls are required.");
            if (string.IsNullOrWhiteSpace(plan.Scope)) errors.Add("Scope is required.");
            if (string.IsNullOrWhiteSpace(plan.PopulationSource)) errors.Add("Population source is required.");
            if (!plan.PopulationCount.HasValue) errors.Add("Population count is required.");
            if (string.IsNullOrWhiteSpace(plan.SamplingMethod)) errors.Add("Sampling method is required.");
            if (!plan.SampleSize.HasValue || plan.SampleSize.Value <= 0) errors.Add("A positive sample size is required.");
            if (plan.PopulationCount.HasValue && plan.SampleSize > plan.PopulationCount) errors.Add("Sample size cannot exceed population count.");
            return errors;
            }

        private static void ValidateNumbers(string paperType, JsonElement details, List<string> errors)
            {
            string[] nonNegative = paperType switch
                {
                WorkingPaperTypes.LoanCase => new[] { "outstandingAmount", "overdueDays", "recordedProvision", "expectedProvision", "requiredSecurityValue", "eligibleSecurityValue" },
                WorkingPaperTypes.Voucher => new[] { "amount", "recalculatedAmount" },
                WorkingPaperTypes.FixedAsset => new[] { "recordedNetBookValue", "expectedNetBookValue" },
                WorkingPaperTypes.CashCount => new[] { "denomination", "physicalQuantity", "registerQuantity", "ledgerBalance" },
                _ => Array.Empty<string>()
                };
            foreach (var field in nonNegative)
                if (details.TryGetProperty(field, out var value) && !IsEmpty(value) && (!TryDecimal(value, out var number) || number < 0))
                    errors.Add($"{field} must be a non-negative number.");
            if (paperType == WorkingPaperTypes.CashCount)
                foreach (var field in new[] { "physicalQuantity", "registerQuantity" })
                    if (details.TryGetProperty(field, out var value) && TryDecimal(value, out var number) && number != Math.Truncate(number))
                        errors.Add($"{field} must be a whole number.");
            }

        private static bool IsEmpty(JsonElement value) => value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            || (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString()));

        private static decimal Decimal(JsonElement details, string name) =>
            details.TryGetProperty(name, out var value) && TryDecimal(value, out var number) ? number : 0m;

        private static bool TryDecimal(JsonElement value, out decimal number)
            {
            if (value.ValueKind == JsonValueKind.Number) return value.TryGetDecimal(out number);
            return decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number);
            }
        }
    }
