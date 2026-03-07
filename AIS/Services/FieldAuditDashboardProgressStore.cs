using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AIS.Services
    {
    public class FieldAuditDashboardProgressStore
        {
        private readonly object _syncRoot = new object();
        private readonly string _storePath;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
            {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
            };

        public FieldAuditDashboardProgressStore(IWebHostEnvironment environment)
            {
            var rootPath = environment?.ContentRootPath ?? AppContext.BaseDirectory;
            var directory = Path.Combine(rootPath, "App_Data");
            Directory.CreateDirectory(directory);
            _storePath = Path.Combine(directory, "fieldaudit-dashboard-progress.json");
            }

        public HashSet<string> GetCompletedStepCodes(int engagementId)
            {
            if (engagementId <= 0)
                {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

            lock (_syncRoot)
                {
                var payload = LoadUnsafe();
                if (!payload.TryGetValue(engagementId, out var records))
                    {
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }

                return new HashSet<string>(records ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                }
            }

        public void MarkCompleted(int engagementId, string stepCode)
            {
            if (engagementId <= 0 || string.IsNullOrWhiteSpace(stepCode))
                {
                return;
                }

            lock (_syncRoot)
                {
                var payload = LoadUnsafe();
                if (!payload.TryGetValue(engagementId, out var records) || records == null)
                    {
                    records = new List<string>();
                    payload[engagementId] = records;
                    }

                if (!records.Any(item => string.Equals(item, stepCode, StringComparison.OrdinalIgnoreCase)))
                    {
                    records.Add(stepCode);
                    SaveUnsafe(payload);
                    }
                }
            }

        private Dictionary<int, List<string>> LoadUnsafe()
            {
            try
                {
                if (!File.Exists(_storePath))
                    {
                    return new Dictionary<int, List<string>>();
                    }

                var json = File.ReadAllText(_storePath);
                if (string.IsNullOrWhiteSpace(json))
                    {
                    return new Dictionary<int, List<string>>();
                    }

                return JsonSerializer.Deserialize<Dictionary<int, List<string>>>(json, _serializerOptions)
                       ?? new Dictionary<int, List<string>>();
                }
            catch
                {
                return new Dictionary<int, List<string>>();
                }
            }

        private void SaveUnsafe(Dictionary<int, List<string>> payload)
            {
            var normalized = payload.ToDictionary(
                item => item.Key,
                item => (item.Value ?? new List<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

            var json = JsonSerializer.Serialize(normalized, _serializerOptions);
            File.WriteAllText(_storePath, json);
            }
        }
    }
