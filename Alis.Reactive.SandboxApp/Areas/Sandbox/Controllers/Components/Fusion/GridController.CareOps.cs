using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Care Operations board: chip fast-filters, checkbox bulk selection, complex
    /// cell editors, frozen columns, sticky header, per-row discharge action link,
    /// bulk reassign/flag, and a templated admit dialog. State is per browser session
    /// (seeded on first access), so edits persist across saves and page refreshes
    /// while each browser/test context stays isolated.
    /// </summary>
    public partial class GridController
    {
        private const string CareSessionKey = "careOpsCensus";

        private static readonly List<string> CareWings = new() { "", "North", "East", "West", "South" };
        private static readonly List<string> CareLevelOptions = new()
        {
            "", "Independent", "Assisted Living", "Memory Care", "Skilled Nursing"
        };
        private static readonly List<string> CareRiskOptions = new() { "", "Low", "Moderate", "High", "Critical" };
        private static readonly List<string> CareNurseOptions = new()
        {
            "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park", "Night Float Team"
        };

        private List<ResidentCareItem> GetCareCensus()
        {
            var json = HttpContext.Session.GetString(CareSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                var seed = GenerateCare();
                HttpContext.Session.SetString(CareSessionKey, JsonSerializer.Serialize(seed));
                return seed;
            }
            return JsonSerializer.Deserialize<List<ResidentCareItem>>(json)!;
        }

        private void SaveCareCensus(List<ResidentCareItem> census) =>
            HttpContext.Session.SetString(CareSessionKey, JsonSerializer.Serialize(census));

        [HttpGet("CareOps")]
        public IActionResult CareOps()
        {
            GetCareCensus(); // seed the session on first visit
            ViewBag.Wings = CareWings;
            ViewBag.CareLevels = CareLevelOptions;
            ViewBag.RiskLevels = CareRiskOptions;
            ViewBag.Nurses = CareNurseOptions;

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareOps.cshtml",
                new CareOpsViewModel());
        }

        [HttpPost("CareOpsData")]
        public IActionResult CareOpsData([FromBody] CareOpsDataRequest? request)
        {
            request ??= new CareOpsDataRequest();
            var rows = FilterCare(GetCareCensus(), request.FilterRisk, request.FilterCareLevel).ToList();
            rows = SortCare(rows, request.Sorted).ToList();

            var total = rows.Count;
            var skip = Math.Max(request.Skip, 0);
            var take = request.Take > 0 ? request.Take : 10;

            return Ok(new CareOpsResponse
            {
                Result = rows.Skip(skip).Take(take).ToList(),
                Count = total,
                Summary = CareSummary(rows, request.FilterRisk, request.FilterCareLevel)
            });
        }

        [HttpPost("CareOpsSave")]
        public IActionResult CareOpsSave([FromBody] CareOpsSaveRequest? request)
        {
            var changed = request?.BatchChanges?.ChangedRecords ?? new List<ResidentCareItem>();
            var overrides = changed.GroupBy(r => r.ResidentId).ToDictionary(g => g.Key, g => g.Last());

            var census = GetCareCensus();
            foreach (var row in census.Where(r => overrides.ContainsKey(r.ResidentId)))
            {
                var edit = overrides[row.ResidentId];
                row.CareLevel = string.IsNullOrWhiteSpace(edit.CareLevel) ? row.CareLevel : edit.CareLevel;
                row.RiskLevel = string.IsNullOrWhiteSpace(edit.RiskLevel) ? row.RiskLevel : edit.RiskLevel;
                row.PrimaryNurse = string.IsNullOrWhiteSpace(edit.PrimaryNurse) ? row.PrimaryNurse : edit.PrimaryNurse;
                row.OpenTasks = edit.OpenTasks;
                row.NextReview = string.IsNullOrWhiteSpace(edit.NextReview) ? row.NextReview : edit.NextReview;
            }
            SaveCareCensus(census);

            return Ok(new CareOpsResponse
            {
                Result = SortCare(census, null).Take(10).ToList(),
                Count = census.Count,
                Summary = overrides.Count == 0
                    ? "no pending care-plan edits to save"
                    : $"saved care-plan updates for {overrides.Count} resident(s)"
            });
        }

        [HttpPost("CareOpsBulkNurse")]
        public IActionResult CareOpsBulkNurse([FromBody] CareOpsBulkRequest? request)
        {
            var nurse = string.IsNullOrWhiteSpace(request?.Nurse) ? "Night Float Team" : request!.Nurse!;
            var ids = (request?.SelectedRecords ?? new()).Select(r => r.ResidentId).ToHashSet();

            var census = GetCareCensus();
            foreach (var row in census.Where(r => ids.Contains(r.ResidentId)))
                row.PrimaryNurse = nurse;
            SaveCareCensus(census);

            return Ok(new CareOpsResponse
            {
                Result = SortCare(census, null).Take(10).ToList(),
                Count = census.Count,
                Summary = ids.Count == 0
                    ? "select residents first, then reassign"
                    : $"reassigned {ids.Count} resident(s) to {nurse}"
            });
        }

        [HttpPost("CareOpsBulkRisk")]
        public IActionResult CareOpsBulkRisk([FromBody] CareOpsBulkRequest? request)
        {
            var risk = string.IsNullOrWhiteSpace(request?.Risk) ? "High" : request!.Risk!;
            var ids = (request?.SelectedRecords ?? new()).Select(r => r.ResidentId).ToHashSet();

            var census = GetCareCensus();
            foreach (var row in census.Where(r => ids.Contains(r.ResidentId)))
                row.RiskLevel = risk;
            SaveCareCensus(census);

            return Ok(new CareOpsResponse
            {
                Result = SortCare(census, null).Take(10).ToList(),
                Count = census.Count,
                Summary = ids.Count == 0
                    ? "select residents first, then flag risk"
                    : $"flagged {ids.Count} resident(s) as {risk} risk"
            });
        }

        [HttpPost("CareOpsDischargeSelected")]
        public IActionResult CareOpsDischargeSelected([FromBody] CareOpsBulkRequest? request)
        {
            var ids = (request?.SelectedRecords ?? new()).Select(r => r.ResidentId).ToHashSet();

            var census = GetCareCensus();
            var discharged = census.Where(r => ids.Contains(r.ResidentId)).Select(r => r.ResidentName).ToList();
            census.RemoveAll(r => ids.Contains(r.ResidentId));
            SaveCareCensus(census);

            return Ok(new CareOpsResponse
            {
                Result = SortCare(census, null).Take(10).ToList(),
                Count = census.Count,
                Summary = ids.Count == 0
                    ? "select a resident first, then discharge"
                    : $"discharged {string.Join(", ", discharged)}"
            });
        }

        [HttpPost("CareOpsAdmit")]
        public IActionResult CareOpsAdmit([FromBody] CareOpsViewModel? request)
        {
            if (request == null)
                return ValidationError(new Dictionary<string, string[]> { ["NewResidentName"] = ["Request body is required."] });

            if (!TryValidate(new CareOpsAdmitValidator(), request, out var error))
                return error;

            var census = GetCareCensus();
            var row = new ResidentCareItem
            {
                ResidentId = census.Count == 0 ? 8000 : census.Max(r => r.ResidentId) + 1,
                ResidentName = request.NewResidentName!,
                Wing = request.NewWing!,
                CareLevel = request.NewCareLevel!,
                RiskLevel = request.NewRiskLevel!,
                PrimaryNurse = "Admissions Desk",
                OpenTasks = (int)request.NewOpenTasks!.Value,
                NextReview = "2026-06-15"
            };
            census.Insert(0, row);
            SaveCareCensus(census);

            return Ok(new CareOpsResponse
            {
                Result = SortCare(census, null).Take(10).ToList(),
                Count = census.Count,
                Summary = $"admitted {row.ResidentName} to {row.Wing} wing at {row.RiskLevel} risk"
            });
        }

        [HttpGet("CareOpsActionRows")]
        public IActionResult CareOpsActionRows()
        {
            var rows = GetCareCensus().Take(8).ToList();
            return PartialView(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/_CareOpsActionRows.cshtml", rows);
        }

        [HttpPost("CareOpsDischargeOne/{id:int}")]
        public IActionResult CareOpsDischargeOne(int id)
        {
            var census = GetCareCensus();
            census.RemoveAll(r => r.ResidentId == id);
            SaveCareCensus(census);
            return Ok(new { discharged = id });
        }

        [HttpPost("CareOpsFlagOne/{id:int}")]
        public IActionResult CareOpsFlagOne(int id)
        {
            var census = GetCareCensus();
            foreach (var row in census.Where(r => r.ResidentId == id))
                row.RiskLevel = "Critical";
            SaveCareCensus(census);
            return Ok(new { flagged = id });
        }

        private static IEnumerable<ResidentCareItem> FilterCare(
            IEnumerable<ResidentCareItem> source, string? risk, string? careLevel)
        {
            var query = source;
            if (!string.IsNullOrWhiteSpace(risk) && !risk.Equals("All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.RiskLevel.Equals(risk, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(careLevel) && !careLevel.Equals("All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.CareLevel.Equals(careLevel, StringComparison.OrdinalIgnoreCase));
            return query;
        }

        private static IEnumerable<ResidentCareItem> SortCare(
            IEnumerable<ResidentCareItem> source, List<GridSortRequest>? sorted)
        {
            if (sorted == null || sorted.Count == 0)
                return source.OrderBy(r => r.ResidentName);

            IOrderedEnumerable<ResidentCareItem>? ordered = null;
            foreach (var sort in sorted.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
            {
                var desc = sort.Direction.Equals("descending", StringComparison.OrdinalIgnoreCase);
                ordered = ordered == null
                    ? desc ? source.OrderByDescending(r => ReadCareSort(r, sort.Name))
                           : source.OrderBy(r => ReadCareSort(r, sort.Name))
                    : desc ? ordered.ThenByDescending(r => ReadCareSort(r, sort.Name))
                           : ordered.ThenBy(r => ReadCareSort(r, sort.Name));
            }
            return ordered ?? source.OrderBy(r => r.ResidentName);
        }

        private static object ReadCareSort(ResidentCareItem r, string field) => field switch
        {
            "residentId" => r.ResidentId,
            "residentName" => r.ResidentName,
            "wing" => r.Wing,
            "careLevel" => r.CareLevel,
            "riskLevel" => r.RiskLevel,
            "primaryNurse" => r.PrimaryNurse,
            "openTasks" => r.OpenTasks,
            "nextReview" => r.NextReview,
            _ => r.ResidentName
        };

        private static string CareSummary(List<ResidentCareItem> rows, string? risk, string? careLevel)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(risk) && !risk.Equals("All", StringComparison.OrdinalIgnoreCase)) parts.Add($"{risk} risk");
            if (!string.IsNullOrWhiteSpace(careLevel) && !careLevel.Equals("All", StringComparison.OrdinalIgnoreCase)) parts.Add(careLevel!);
            var openTasks = rows.Sum(r => r.OpenTasks);
            var scope = parts.Count == 0 ? "whole census" : string.Join(", ", parts);
            return $"{rows.Count} resident(s) — {scope}; {openTasks} open care tasks";
        }

        private static List<ResidentCareItem> GenerateCare()
        {
            var names = new[]
            {
                "Amina Patel", "Grace Bennett", "Henry Liu", "Irene Morgan",
                "Jonah Reed", "Katherine Ortiz", "Leo Simmons", "Mara Thompson",
                "Noah Walsh", "Priya Shah", "Ruth Carter", "Samuel Diaz",
                "Tara Novak", "Victor Chen", "Wendy Price", "Yara Ahmed",
                "Bilal Khan", "Clara Hoffman", "Diego Reyes", "Esther Cole"
            };
            var careLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
            var wings = new[] { "North", "East", "West", "South" };
            var risks = new[] { "Low", "Moderate", "High", "Critical" };
            var nurses = new[] { "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park" };

            var rows = new List<ResidentCareItem>();
            for (var i = 0; i < 30; i++)
            {
                rows.Add(new ResidentCareItem
                {
                    ResidentId = 7000 + i,
                    ResidentName = names[i % names.Length],
                    Wing = wings[(i / 2) % wings.Length],
                    CareLevel = careLevels[i % careLevels.Length],
                    RiskLevel = risks[(i + (i % 3)) % risks.Length],
                    PrimaryNurse = nurses[i % nurses.Length],
                    OpenTasks = i * 3 % 9,
                    NextReview = new DateOnly(2026, 6, 1).AddDays(i % 21).ToString("yyyy-MM-dd")
                });
            }
            return rows;
        }
    }
}
