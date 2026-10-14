using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Care operations: the director of nursing narrows the care census by risk or care level, edits care
    /// levels, risks, nurses, open tasks and review days in the grid, reassigns, flags or discharges the
    /// residents ticked on the board, admits a resident, and works the high-risk watch list. The census lives
    /// in the browser session (seeded on first visit), so each browser keeps its own board while its changes
    /// persist.
    /// </summary>
    public partial class GridController
    {
        private const string CareSessionKey = "careOpsCensus";
        private const int CarePageSize = 10;

        [HttpGet("CareOps")]
        public IActionResult CareOps()
        {
            GetCareCensus();
            return View("~/Areas/Sandbox/Views/Components/Fusion/Grid/CareOps.cshtml", new CareOpsViewModel());
        }

        [HttpPost("CareOpsData")]
        public IActionResult CareOpsData([FromBody] CareOpsDataRequest request)
        {
            var risk = CareFilter(request.FilterRisk);
            var careLevel = CareFilter(request.FilterCareLevel);
            var rows = SortCare(GetCareCensus()
                .Where(r => risk == null || r.RiskLevel == risk)
                .Where(r => careLevel == null || r.CareLevel == careLevel), request.Sorted).ToList();

            return Ok(new CareOpsResponse
            {
                Result = rows.Skip(request.Skip).Take(request.Take).ToList(),
                Count = rows.Count,
                Summary = CareSummary(rows, risk, careLevel)
            });
        }

        // Every edited row answers to the whole validator, including the rule the grid's cells cannot check;
        // one refused row keeps the census as it was.
        [HttpPost("CareOpsSave")]
        public IActionResult CareOpsSave([FromBody] CareOpsSaveRequest request)
        {
            var edits = (request.BatchChanges?.ChangedRecords ?? new List<ResidentCareItem>())
                .GroupBy(r => r.ResidentId)
                .ToDictionary(g => g.Key, g => g.Last());

            var census = GetCareCensus();
            var edited = census.Where(r => edits.ContainsKey(r.ResidentId)).ToList();
            foreach (var row in edited)
            {
                var edit = edits[row.ResidentId];
                row.CareLevel = edit.CareLevel;
                row.RiskLevel = edit.RiskLevel;
                row.PrimaryNurse = edit.PrimaryNurse;
                row.OpenTasks = edit.OpenTasks;
                row.NextReview = edit.NextReview;
            }

            var validator = new ResidentCareItemValidator();
            foreach (var row in edited)
            {
                var result = validator.Validate(row);
                if (!result.IsValid)
                    return BadRequest(new CareOpsResponse { Summary = $"{row.ResidentName}: {result.Errors[0].ErrorMessage}" });
            }
            SaveCareCensus(census);

            return CareBoard(census, edited.Count switch
            {
                0 => "There are no care-plan changes to save.",
                1 => $"Saved care-plan changes for {edited[0].ResidentName}.",
                _ => $"Saved care-plan changes for {edited.Count} residents."
            });
        }

        [HttpPost("CareOpsBulkNurse")]
        public IActionResult CareOpsBulkNurse([FromBody] CareOpsBulkRequest request)
        {
            var census = GetCareCensus();
            var reassigned = CarePicked(census, request);
            foreach (var row in reassigned)
                row.PrimaryNurse = request.Nurse!;
            SaveCareCensus(census);

            return CareBoard(census, reassigned.Count == 0
                ? "Tick the residents to reassign first."
                : $"Reassigned {CareNames(reassigned)} to the {request.Nurse}.");
        }

        [HttpPost("CareOpsBulkRisk")]
        public IActionResult CareOpsBulkRisk([FromBody] CareOpsBulkRequest request)
        {
            var census = GetCareCensus();
            var flagged = CarePicked(census, request);
            foreach (var row in flagged)
                row.RiskLevel = request.Risk!;
            SaveCareCensus(census);

            return CareBoard(census, flagged.Count == 0
                ? "Tick the residents to flag first."
                : $"Flagged {CareNames(flagged)} at {request.Risk} risk.");
        }

        [HttpPost("CareOpsDischargeSelected")]
        public IActionResult CareOpsDischargeSelected([FromBody] CareOpsBulkRequest request)
        {
            var census = GetCareCensus();
            var discharged = CarePicked(census, request);
            census.RemoveAll(discharged.Contains);
            SaveCareCensus(census);

            return CareBoard(census, discharged.Count == 0
                ? "Tick the residents to discharge first."
                : $"Discharged {CareNames(discharged)}.");
        }

        // A new resident is covered by the night float team until the charge nurse assigns a primary nurse,
        // and has a care review a week after admission.
        [HttpPost("CareOpsAdmit")]
        public IActionResult CareOpsAdmit([FromBody] CareOpsViewModel request)
        {
            if (!TryValidate(new CareOpsAdmitValidator(), request, out var error))
                return error;

            var census = GetCareCensus();
            var row = new ResidentCareItem
            {
                ResidentId = census.Max(r => r.ResidentId) + 1,
                ResidentName = request.NewResidentName!.Trim(),
                Wing = request.NewWing!,
                CareLevel = request.NewCareLevel!,
                RiskLevel = request.NewRiskLevel!,
                PrimaryNurse = "Night Float Team",
                OpenTasks = (int)request.NewOpenTasks!.Value,
                NextReview = "2026-10-21"
            };
            census.Add(row);
            SaveCareCensus(census);

            return CareBoard(census, $"Admitted {row.ResidentName} to {row.Wing} wing, {row.CareLevel}, at {row.RiskLevel} risk.");
        }

        // The high-risk watch list: each resident at high risk, by name, with their quick actions.
        [HttpGet("CareOpsActionRows")]
        public IActionResult CareOpsActionRows() =>
            PartialView(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/_CareOpsActionRows.cshtml",
                GetCareCensus().Where(r => r.RiskLevel == "High").OrderBy(r => r.ResidentName, StringComparer.Ordinal).ToList());

        [HttpPost("CareOpsDischargeOne/{id:int}")]
        public IActionResult CareOpsDischargeOne(int id)
        {
            var census = GetCareCensus();
            census.RemoveAll(r => r.ResidentId == id);
            SaveCareCensus(census);
            return NoContent();
        }

        [HttpPost("CareOpsFlagOne/{id:int}")]
        public IActionResult CareOpsFlagOne(int id)
        {
            var census = GetCareCensus();
            foreach (var row in census.Where(r => r.ResidentId == id))
                row.RiskLevel = "Critical";
            SaveCareCensus(census);
            return NoContent();
        }

        private List<ResidentCareItem> GetCareCensus()
        {
            var json = HttpContext.Session.GetString(CareSessionKey);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<ResidentCareItem>>(json)!;

            var seed = CareCensusOnFile();
            SaveCareCensus(seed);
            return seed;
        }

        private void SaveCareCensus(List<ResidentCareItem> census) =>
            HttpContext.Session.SetString(CareSessionKey, JsonSerializer.Serialize(census));

        // After a change the board shows its first page again, by name, with what the change did.
        private OkObjectResult CareBoard(List<ResidentCareItem> census, string summary)
        {
            var rows = SortCare(census, null).ToList();
            return Ok(new CareOpsResponse { Result = rows.Take(CarePageSize).ToList(), Count = rows.Count, Summary = summary });
        }

        private static List<ResidentCareItem> CarePicked(List<ResidentCareItem> census, CareOpsBulkRequest request)
        {
            var picked = (request.SelectedRecords ?? new List<ResidentCareItem>()).Select(r => r.ResidentId).ToHashSet();
            return census.Where(r => picked.Contains(r.ResidentId)).OrderBy(r => r.ResidentName, StringComparer.Ordinal).ToList();
        }

        // "Amina Patel", "Amina Patel and Arthur Bell", "Amina Patel, Arthur Bell and Bilal Khan".
        private static string CareNames(List<ResidentCareItem> residents) =>
            residents.Count == 1
                ? residents[0].ResidentName
                : $"{string.Join(", ", residents.Take(residents.Count - 1).Select(r => r.ResidentName))} and {residents[^1].ResidentName}";

        // A chip's text, where "All" chooses no filter.
        private static string? CareFilter(string? chip) =>
            string.IsNullOrWhiteSpace(chip) || chip == "All" ? null : chip;

        // The grid's sort, column by column; a board with no sort lists residents by name.
        private static IEnumerable<ResidentCareItem> SortCare(
            IEnumerable<ResidentCareItem> source, List<FusionGridSortColumn>? sorted)
        {
            IOrderedEnumerable<ResidentCareItem>? ordered = null;
            foreach (var sort in sorted ?? new List<FusionGridSortColumn>())
            {
                var descending = sort.Direction.Equals("descending", StringComparison.OrdinalIgnoreCase);
                ordered = (ordered, descending) switch
                {
                    (null, false) => source.OrderBy(r => ReadCareSort(r, sort.Name)),
                    (null, true) => source.OrderByDescending(r => ReadCareSort(r, sort.Name)),
                    (_, false) => ordered.ThenBy(r => ReadCareSort(r, sort.Name)),
                    _ => ordered.ThenByDescending(r => ReadCareSort(r, sort.Name))
                };
            }
            return ordered ?? source.OrderBy(r => r.ResidentName, StringComparer.Ordinal);
        }

        private static IComparable ReadCareSort(ResidentCareItem r, string field) => field switch
        {
            "residentId" => r.ResidentId,
            "wing" => r.Wing,
            "careLevel" => r.CareLevel,
            "riskLevel" => r.RiskLevel,
            "primaryNurse" => r.PrimaryNurse,
            "openTasks" => r.OpenTasks,
            "nextReview" => r.NextReview,
            _ => r.ResidentName
        };

        // "24 residents on the census: 62 open care tasks.", "4 residents at Critical risk: …",
        // "7 Memory Care residents: …".
        private static string CareSummary(List<ResidentCareItem> rows, string? risk, string? careLevel)
        {
            var residents = rows.Count == 1 ? "resident" : "residents";
            var who = $"{rows.Count} {(careLevel == null ? "" : careLevel + " ")}{residents}"
                + (risk != null ? $" at {risk} risk" : careLevel == null ? " on the census" : "");
            var tasks = rows.Sum(r => r.OpenTasks);
            return $"{who}: {tasks} open care {(tasks == 1 ? "task" : "tasks")}.";
        }

        private static ResidentCareItem CareResident(int id, string name, string wing, string careLevel, string risk, string nurse, int openTasks, string nextReview) =>
            new() { ResidentId = id, ResidentName = name, Wing = wing, CareLevel = careLevel, RiskLevel = risk, PrimaryNurse = nurse, OpenTasks = openTasks, NextReview = nextReview };

        private static List<ResidentCareItem> CareCensusOnFile() => new()
        {
            CareResident(7001, "Abigail Moore", "North", "Independent", "Low", "Nora Ellis", 1, "2026-10-21"),
            CareResident(7002, "Amina Patel", "North", "Memory Care", "High", "Malik Stone", 6, "2026-10-16"),
            CareResident(7003, "Arthur Bell", "East", "Assisted Living", "Moderate", "Elena Ruiz", 2, "2026-10-27"),
            CareResident(7004, "Beatrice Lane", "East", "Skilled Nursing", "Critical", "Owen Park", 5, "2026-10-15"),
            CareResident(7005, "Bilal Khan", "West", "Memory Care", "High", "Nora Ellis", 3, "2026-10-22"),
            CareResident(7006, "Clara Hoffman", "West", "Skilled Nursing", "Critical", "Malik Stone", 4, "2026-10-19"),
            CareResident(7007, "Daniel Reyes", "South", "Independent", "Low", "Elena Ruiz", 0, "2026-11-03"),
            CareResident(7008, "Dorothy Fleming", "South", "Assisted Living", "Moderate", "Owen Park", 2, "2026-10-28"),
            CareResident(7009, "Edith Morris", "North", "Memory Care", "Moderate", "Nora Ellis", 3, "2026-10-23"),
            CareResident(7010, "Esther Cole", "North", "Assisted Living", "Low", "Malik Stone", 1, "2026-11-04"),
            CareResident(7011, "Frank Delgado", "East", "Skilled Nursing", "High", "Elena Ruiz", 5, "2026-10-20"),
            CareResident(7012, "Grace Bennett", "East", "Independent", "Low", "Owen Park", 0, "2026-11-05"),
            CareResident(7013, "Harold Chen", "West", "Memory Care", "Critical", "Nora Ellis", 6, "2026-10-26"),
            CareResident(7014, "Henry Liu", "West", "Assisted Living", "Moderate", "Malik Stone", 2, "2026-10-29"),
            CareResident(7015, "Irene Morgan", "South", "Skilled Nursing", "High", "Elena Ruiz", 4, "2026-10-30"),
            CareResident(7016, "Jonah Reed", "South", "Independent", "Low", "Owen Park", 1, "2026-11-06"),
            CareResident(7017, "Katherine Ortiz", "North", "Memory Care", "Moderate", "Nora Ellis", 2, "2026-11-02"),
            CareResident(7018, "Leo Simmons", "North", "Assisted Living", "Low", "Malik Stone", 0, "2026-11-09"),
            CareResident(7019, "Mara Thompson", "East", "Skilled Nursing", "Critical", "Elena Ruiz", 3, "2026-11-10"),
            CareResident(7020, "Noah Walsh", "East", "Independent", "Moderate", "Owen Park", 1, "2026-11-11"),
            CareResident(7021, "Priya Shah", "West", "Memory Care", "High", "Nora Ellis", 5, "2026-11-12"),
            CareResident(7022, "Ruth Carter", "West", "Assisted Living", "Low", "Malik Stone", 2, "2026-11-13"),
            CareResident(7023, "Samuel Diaz", "South", "Skilled Nursing", "Moderate", "Elena Ruiz", 3, "2026-11-16"),
            CareResident(7024, "Yara Ahmed", "South", "Memory Care", "Low", "Owen Park", 1, "2026-11-17")
        };
    }
}
