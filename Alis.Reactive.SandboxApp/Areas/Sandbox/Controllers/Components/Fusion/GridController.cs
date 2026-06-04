using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Grid")]
    public partial class GridController : Controller
    {
        private static readonly List<ResidentGridItem> AllResidents = GenerateResidents();
        private static readonly List<ResidentDirectoryRecord> ResidentDirectory = GenerateResidentDirectory();

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml", new GridModel());
        }

        [HttpGet("Directory")]
        public IActionResult Directory()
        {
            ViewBag.CareLevels = new List<string>
            {
                "", "Independent", "Assisted Living", "Memory Care", "Skilled Nursing"
            };
            ViewBag.RiskLevels = new List<string>
            {
                "", "Low", "Moderate", "High"
            };

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
                new ResidentDirectoryModel());
        }

        [HttpGet("Editing")]
        public IActionResult Editing()
        {
            ViewBag.EditRows = ResidentDirectory.Take(16).Select(ToGridItem).ToList();
            ViewBag.RiskLevels = new List<string> { "", "Low", "Moderate", "High" };

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Editing.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("Operations")]
        public IActionResult Operations()
        {
            ViewBag.RiskLevels = new List<string> { "", "Low", "Moderate", "High" };

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Operations.cshtml",
                new GridOperationsModel
                {
                    PatchRiskLevel = "High",
                    PatchOpenTasks = 6
                });
        }

        [HttpGet("EditingRows")]
        public IActionResult EditingRows()
        {
            return Ok(ResidentDirectory.Take(16).Select(ToGridItem).ToList());
        }

        [HttpGet("OperationRows")]
        public IActionResult OperationRows()
        {
            return Ok(ResidentDirectory.Take(12).Select(ToGridItem).ToList());
        }

        /// <summary>
        /// Server-side grid data endpoint. Accepts the full grid state:
        /// skip/take for paging, sorted[] for multi-column sort, minAge for filtering.
        /// Returns {result, count} for Syncfusion Grid custom binding.
        /// </summary>
        [HttpPost("Data")]
        public IActionResult Data([FromBody] GridDataRequest? request)
        {
            request ??= new GridDataRequest();
            var query = AllResidents.AsEnumerable();

            if (request.MinAge.HasValue)
                query = query.Where(r => r.Age >= (int)request.MinAge.Value);

            if (request.Sorted != null)
            {
                var first = true;
                foreach (var sort in request.Sorted)
                {
                    var prop = typeof(ResidentGridItem).GetProperty(
                        sort.Name,
                        System.Reflection.BindingFlags.IgnoreCase
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance);

                    if (prop == null) continue;

                    if (first)
                    {
                        query = sort.Direction == "descending"
                            ? query.OrderByDescending(r => prop.GetValue(r))
                            : query.OrderBy(r => prop.GetValue(r));
                        first = false;
                    }
                    else
                    {
                        var ordered = (IOrderedEnumerable<ResidentGridItem>)query;
                        query = sort.Direction == "descending"
                            ? ordered.ThenByDescending(r => prop.GetValue(r))
                            : ordered.ThenBy(r => prop.GetValue(r));
                    }
                }
            }

            var total = query.Count();
            var paged = query.Skip(request.Skip).Take(request.Take > 0 ? request.Take : 10).ToList();

            return Ok(new ResidentGridResponse { Result = paged, Count = total });
        }

        [HttpPost("DirectoryData")]
        public IActionResult DirectoryData([FromBody] ResidentDirectoryRequest? request)
        {
            request ??= new ResidentDirectoryRequest();
            var rows = ApplyDirectoryRequest(ResidentDirectory, request).ToList();
            var total = rows.Count;

            if (request.Group is { Count: > 0 })
            {
                var groupedRows = GroupDirectoryRows(rows, request.Group, 0);
                return Ok(new ResidentDirectoryResponse
                {
                    Result = groupedRows,
                    Count = total,
                    Summary = $"{total} residents grouped by {ToDisplayName(request.Group[0])}"
                });
            }

            var skip = Math.Max(request.Skip, 0);
            var take = request.Take > 0 ? request.Take : 8;
            var page = rows.Skip(skip).Take(take).Select(ToGridItem).ToList();

            return Ok(new ResidentDirectoryResponse
            {
                Result = page,
                Count = total,
                Summary = $"{total} residents matched"
            });
        }

        [HttpPost("SelectResident")]
        public IActionResult SelectResident([FromBody] ResidentSelectionRequest request)
        {
            var resident = ResidentDirectory.FirstOrDefault(r => r.ResidentId == request.ResidentId);
            if (resident == null)
            {
                return Ok(new ResidentDirectorySelectionResponse
                {
                    Summary = $"No resident selected at row {request.RowIndex}"
                });
            }

            return Ok(new ResidentDirectorySelectionResponse
            {
                ResidentName = resident.ResidentName,
                Summary =
                    $"{resident.ResidentName}: {resident.CareLevel}, {resident.Wing} wing, {resident.OpenTasks} open tasks"
            });
        }

        [HttpPost("SelectionIndexes")]
        public IActionResult SelectionIndexes([FromBody] ResidentSelectionIndexesRequest request)
        {
            var indexes = request.SelectedRowIndexes ?? new List<int>();
            return Ok(new ResidentDirectorySelectionResponse
            {
                Summary = indexes.Count == 0
                    ? "no selected row indexes"
                    : $"selected row indexes: {string.Join(", ", indexes)}"
            });
        }

        [HttpPost("SelectionRecords")]
        public IActionResult SelectionRecords([FromBody] ResidentSelectionRecordsRequest request)
        {
            var records = request.SelectedRecords ?? new List<ResidentDirectoryGridItem>();
            return Ok(new ResidentDirectorySelectionResponse
            {
                Summary = records.Count == 0
                    ? "no selected records"
                    : $"selected records: {string.Join(", ", records.Select(r => r.ResidentName))}"
            });
        }

        [HttpPost("CurrentViewSummary")]
        public IActionResult CurrentViewSummary([FromBody] ResidentCurrentViewRequest request)
        {
            var records = request.Records ?? new List<ResidentDirectoryGridItem>();
            var highRisk = records.Count(r => r.RiskLevel.Equals("High", StringComparison.OrdinalIgnoreCase));
            var lead = records.FirstOrDefault()?.ResidentName ?? "no residents";

            return Ok(new ResidentDirectorySelectionResponse
            {
                Summary = records.Count == 0
                    ? "current view has no residents"
                    : $"current view has {records.Count} residents, {highRisk} high risk; first is {lead}"
            });
        }

        [HttpPost("RowIndexSummary")]
        public IActionResult RowIndexSummary([FromBody] ResidentRowIndexRequest request)
        {
            return Ok(new ResidentDirectorySelectionResponse
            {
                Summary = request.RowIndex < 0
                    ? "resident 6005 is not visible"
                    : $"resident 6005 is visible at row index {request.RowIndex}"
            });
        }

        [HttpPost("ReviewResident")]
        public IActionResult ReviewResident([FromBody] GridTemplateActionPayload request)
        {
            var resident = ResidentDirectory.FirstOrDefault(r => r.ResidentId == request.Id);
            return Ok(new ResidentDirectorySelectionResponse
            {
                ResidentName = resident?.ResidentName ?? "",
                Summary = resident == null
                    ? $"resident {request.Id} was not found"
                    : $"review started for {resident.ResidentName} in suite {resident.Suite}"
            });
        }

        [HttpPost("PatchResidentRow")]
        public IActionResult PatchResidentRow([FromBody] GridOperationsModel? request)
        {
            request ??= new GridOperationsModel();
            var source = ResidentDirectory[5];
            var risk = string.IsNullOrWhiteSpace(request.PatchRiskLevel)
                ? "High"
                : request.PatchRiskLevel;
            var tasks = request.PatchOpenTasks.HasValue
                ? (int)request.PatchOpenTasks.Value
                : 6;

            var row = ToGridItem(source);
            row.ResidentName = "Lena Server Patch";
            row.RiskLevel = risk!;
            row.OpenTasks = tasks;
            row.PrimaryNurse = "Clinical Review Team";
            row.NextReviewDate = "2026-06-30";

            return Ok(new ResidentGridOperationsResponse
            {
                Row = row,
                Summary = $"{row.ResidentName} changed to {row.RiskLevel} risk with {row.OpenTasks} tasks"
            });
        }

        [HttpPost("BatchSummary")]
        public IActionResult BatchSummary([FromBody] ResidentGridBatchSummaryRequest request)
        {
            var changes = request.BatchChanges ?? new FusionGridBatchChanges<ResidentDirectoryGridItem>();
            return Ok(new ResidentGridBatchSummaryResponse
            {
                Summary =
                    $"batch added {changes.AddedRecords.Count}, changed {changes.ChangedRecords.Count}, deleted {changes.DeletedRecords.Count}"
            });
        }

        [HttpPost("CreateEditResident")]
        public IActionResult CreateEditResident()
        {
            var row = new ResidentDirectoryGridItem
            {
                ResidentId = 7200,
                ResidentName = "Sofia Server",
                Age = 79,
                CareLevel = "Assisted Living",
                Wing = "East",
                Suite = "E-512",
                RiskLevel = "Moderate",
                PrimaryNurse = "Elena Ruiz",
                OpenTasks = 2,
                NextReviewDate = "2026-06-24"
            };

            return Ok(new ResidentGridEditResponse
            {
                Row = row,
                Summary = $"{row.ResidentName} loaded from the server"
            });
        }

        [HttpPost("ValidateEditDraft")]
        public IActionResult ValidateEditDraft([FromBody] ResidentGridEditingModel? request)
        {
            if (request == null)
                return ValidationError(new Dictionary<string, string[]> { ["ResidentName"] = ["Request body is required."] });

            if (!TryValidate(new ResidentGridEditingValidator(), request, out var error))
                return error;

            var row = new ResidentDirectoryGridItem
            {
                ResidentId = 6000,
                ResidentName = request.ResidentName!,
                Age = 83,
                CareLevel = "Memory Care",
                Wing = "North",
                Suite = "N-401",
                RiskLevel = request.RiskLevel!,
                PrimaryNurse = "Nora Ellis",
                OpenTasks = (int)request.OpenTasks!.Value,
                NextReviewDate = "2026-06-18"
            };

            return Ok(new ResidentGridEditResponse
            {
                Row = row,
                Summary = $"{row.ResidentName} passed validation and updated row 1"
            });
        }

        private static List<ResidentGridItem> GenerateResidents()
        {
            var firstNames = new[]
            {
                "Alice", "Bob", "Carol", "David", "Eve", "Frank", "Grace", "Henry",
                "Irene", "Jack", "Karen", "Leo", "Maria", "Nathan", "Olivia", "Paul",
                "Quinn", "Ruth", "Samuel", "Tina", "Uma", "Victor", "Wendy", "Xavier",
                "Yvonne", "Zachary"
            };
            var lastNames = new[]
            {
                "Johnson", "Smith", "Davis", "Wilson", "Martinez", "Brown", "Lee", "Taylor",
                "Anderson", "Thomas", "White", "Harris", "Clark", "Lewis", "Walker", "Young",
                "King", "Wright", "Hill", "Scott", "Green", "Adams", "Baker", "Nelson",
                "Carter", "Mitchell"
            };
            var careLevels = new[] { "Independent", "Assisted", "Memory Care", "Skilled Nursing" };
            var wings = new[] { "East", "West", "North", "South" };

            var residents = new List<ResidentGridItem>();
            var rng = new Random(42);

            for (var i = 0; i < 200; i++)
            {
                residents.Add(new ResidentGridItem
                {
                    Name = $"{firstNames[rng.Next(firstNames.Length)]} {lastNames[rng.Next(lastNames.Length)]}",
                    Age = rng.Next(65, 99),
                    CareLevel = careLevels[rng.Next(careLevels.Length)],
                    Wing = wings[rng.Next(wings.Length)]
                });
            }

            return residents;
        }

        private static IEnumerable<ResidentDirectoryRecord> ApplyDirectoryRequest(
            IEnumerable<ResidentDirectoryRecord> source,
            ResidentDirectoryRequest request)
        {
            var query = source;

            if (!string.IsNullOrWhiteSpace(request.ResidentSearch))
                query = query.Where(r => r.ResidentName.Contains(
                    request.ResidentSearch,
                    StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.CareLevel))
                query = query.Where(r => r.CareLevel.Equals(
                    request.CareLevel,
                    StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.RiskLevel))
                query = query.Where(r => r.RiskLevel.Equals(
                    request.RiskLevel,
                    StringComparison.OrdinalIgnoreCase));

            if (request.MinimumAge.HasValue)
                query = query.Where(r => r.Age >= (int)request.MinimumAge.Value);

            query = ApplyGridFilters(query, request.Where);
            query = ApplyGridSearch(query, request.Search);
            query = ApplyGridSorting(query, request.Sorted);

            return query;
        }

        private static IEnumerable<ResidentDirectoryRecord> ApplyGridSearch(
            IEnumerable<ResidentDirectoryRecord> source,
            List<ResidentDirectorySearchRequest>? search)
        {
            var active = search?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Key));
            if (active == null)
                return source;

            var fields = active.Fields is { Count: > 0 }
                ? active.Fields
                : new List<string>
                {
                    "residentName", "careLevel", "wing", "suite", "riskLevel", "primaryNurse"
                };

            return source.Where(row => fields.Any(field =>
                ReadDirectoryText(row, field).Contains(active.Key!, StringComparison.OrdinalIgnoreCase)));
        }

        private static IEnumerable<ResidentDirectoryRecord> ApplyGridFilters(
            IEnumerable<ResidentDirectoryRecord> source,
            List<FusionGridTextFilterCriterion>? filters)
        {
            var flattened = FlattenGridFilters(filters).ToList();
            if (flattened.Count == 0)
                return source;

            return source.Where(row => flattened.All(filter =>
            {
                if (string.IsNullOrWhiteSpace(filter.Field) || string.IsNullOrWhiteSpace(filter.Value))
                    return true;

                var actual = ReadDirectoryText(row, filter.Field);
                var comparison = filter.MatchCase
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                return filter.Operator switch
                {
                    "equal" => actual.Equals(filter.Value, comparison),
                    "notequal" => !actual.Equals(filter.Value, comparison),
                    "startswith" => actual.StartsWith(filter.Value, comparison),
                    "endswith" => actual.EndsWith(filter.Value, comparison),
                    _ => actual.Contains(filter.Value, comparison)
                };
            }));
        }

        private static IEnumerable<FusionGridTextFilterCriterion> FlattenGridFilters(
            IEnumerable<FusionGridTextFilterCriterion>? filters)
        {
            if (filters == null)
                yield break;

            foreach (var filter in filters)
            {
                if (filter.Predicates is { Count: > 0 })
                {
                    foreach (var child in FlattenGridFilters(filter.Predicates))
                        yield return child;
                }
                else
                {
                    yield return filter;
                }
            }
        }

        private static IEnumerable<ResidentDirectoryRecord> ApplyGridSorting(
            IEnumerable<ResidentDirectoryRecord> source,
            List<GridSortRequest>? sorted)
        {
            if (sorted == null || sorted.Count == 0)
                return source.OrderBy(r => r.ResidentName);

            IOrderedEnumerable<ResidentDirectoryRecord>? ordered = null;
            foreach (var sort in sorted.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
            {
                var descending = sort.Direction.Equals("descending", StringComparison.OrdinalIgnoreCase);
                ordered = ordered == null
                    ? descending
                        ? source.OrderByDescending(row => ReadDirectorySortValue(row, sort.Name))
                        : source.OrderBy(row => ReadDirectorySortValue(row, sort.Name))
                    : descending
                        ? ordered.ThenByDescending(row => ReadDirectorySortValue(row, sort.Name))
                        : ordered.ThenBy(row => ReadDirectorySortValue(row, sort.Name));
            }

            return ordered ?? source.OrderBy(r => r.ResidentName);
        }

        private static List<ResidentDirectoryGridItem> GroupDirectoryRows(
            List<ResidentDirectoryRecord> rows,
            List<string> groupFields,
            int level)
        {
            if (level >= groupFields.Count)
                return rows.Select(ToGridItem).ToList();

            var field = groupFields[level];
            return rows
                .GroupBy(row => ReadDirectoryText(row, field))
                .OrderBy(group => group.Key)
                .Select(group => new ResidentDirectoryGridItem
                {
                    Key = group.Key,
                    Count = group.Count(),
                    Field = field,
                    Items = GroupDirectoryRows(group.ToList(), groupFields, level + 1)
                })
                .ToList();
        }

        private static object ReadDirectorySortValue(ResidentDirectoryRecord row, string field) =>
            field switch
            {
                "residentId" => row.ResidentId,
                "residentName" => row.ResidentName,
                "age" => row.Age,
                "careLevel" => row.CareLevel,
                "wing" => row.Wing,
                "suite" => row.Suite,
                "riskLevel" => row.RiskLevel,
                "primaryNurse" => row.PrimaryNurse,
                "openTasks" => row.OpenTasks,
                "nextReviewDate" => row.NextReviewDate,
                _ => row.ResidentName
            };

        private static string ReadDirectoryText(ResidentDirectoryRecord row, string field) =>
            field switch
            {
                "residentId" => row.ResidentId.ToString(),
                "residentName" => row.ResidentName,
                "age" => row.Age.ToString(),
                "careLevel" => row.CareLevel,
                "wing" => row.Wing,
                "suite" => row.Suite,
                "riskLevel" => row.RiskLevel,
                "primaryNurse" => row.PrimaryNurse,
                "openTasks" => row.OpenTasks.ToString(),
                "nextReviewDate" => row.NextReviewDate.ToString("yyyy-MM-dd"),
                _ => ""
            };

        private static string ToDisplayName(string field) =>
            field switch
            {
                "careLevel" => "care level",
                "riskLevel" => "risk level",
                "wing" => "wing",
                _ => field
            };

        private static ResidentDirectoryGridItem ToGridItem(ResidentDirectoryRecord row) =>
            new ResidentDirectoryGridItem
            {
                ResidentId = row.ResidentId,
                ResidentName = row.ResidentName,
                Age = row.Age,
                CareLevel = row.CareLevel,
                Wing = row.Wing,
                Suite = row.Suite,
                RiskLevel = row.RiskLevel,
                PrimaryNurse = row.PrimaryNurse,
                OpenTasks = row.OpenTasks,
                NextReviewDate = row.NextReviewDate.ToString("yyyy-MM-dd")
            };

        private static List<ResidentDirectoryRecord> GenerateResidentDirectory()
        {
            var names = new[]
            {
                "Amina Patel", "Grace Bennett", "Henry Liu", "Irene Morgan",
                "Jonah Reed", "Katherine Ortiz", "Leo Simmons", "Mara Thompson",
                "Noah Walsh", "Priya Shah", "Ruth Carter", "Samuel Diaz",
                "Tara Novak", "Victor Chen", "Wendy Price", "Yara Ahmed"
            };
            var careLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
            var wings = new[] { "North", "East", "West", "South" };
            var suites = new[] { "101", "112", "118", "205", "211", "301", "312", "401" };
            var risks = new[] { "Low", "Moderate", "High" };
            var nurses = new[] { "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park" };

            var rows = new List<ResidentDirectoryRecord>();
            for (var i = 0; i < 240; i++)
            {
                var careLevel = careLevels[i % careLevels.Length];
                var wing = wings[(i / 2) % wings.Length];
                var risk = risks[(i + (careLevel == "Memory Care" ? 1 : 0)) % risks.Length];
                rows.Add(new ResidentDirectoryRecord(
                    6000 + i,
                    names[i % names.Length],
                    67 + (i * 7 % 29),
                    careLevel,
                    wing,
                    $"{wing[0]}-{suites[i % suites.Length]}",
                    risk,
                    nurses[i % nurses.Length],
                    i * 3 % 8,
                    new DateOnly(2026, 6, 1).AddDays(i % 21)));
            }

            return rows;
        }

        private static IActionResult ValidationError(Dictionary<string, string[]> errors) =>
            new BadRequestObjectResult(new { errors });

        private static bool TryValidate<T>(IValidator<T> validator, T model, out IActionResult error)
        {
            var result = validator.Validate(model);
            if (result.IsValid)
            {
                error = null!;
                return true;
            }

            var errors = new Dictionary<string, string[]>();
            foreach (var failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var existing))
                    errors[failure.PropertyName] = [failure.ErrorMessage];
                else
                    errors[failure.PropertyName] = [.. existing, failure.ErrorMessage];
            }

            error = ValidationError(errors);
            return false;
        }
    }

    public class GridDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<GridSortRequest>? Sorted { get; set; }
        public decimal? MinAge { get; set; }
    }

    public class GridSortRequest
    {
        public string Name { get; set; } = "";
        public string Direction { get; set; } = "";
    }

    public class ResidentDirectoryRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<GridSortRequest>? Sorted { get; set; }
        public List<string>? Group { get; set; }
        public List<FusionGridTextFilterCriterion>? Where { get; set; }
        public List<ResidentDirectorySearchRequest>? Search { get; set; }
        public string? ResidentSearch { get; set; }
        public string? CareLevel { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? MinimumAge { get; set; }
    }

    public class ResidentDirectorySearchRequest
    {
        public List<string>? Fields { get; set; }
        public string? Key { get; set; }
        public string? Operator { get; set; }
        public bool IgnoreCase { get; set; }
    }

    public class ResidentSelectionRequest
    {
        public int ResidentId { get; set; }
        public int RowIndex { get; set; }
    }

    public class ResidentSelectionIndexesRequest
    {
        public List<int>? SelectedRowIndexes { get; set; }
    }

    public class ResidentSelectionRecordsRequest
    {
        public List<ResidentDirectoryGridItem>? SelectedRecords { get; set; }
    }

    public class ResidentCurrentViewRequest
    {
        public List<ResidentDirectoryGridItem>? Records { get; set; }
    }

    public class ResidentRowIndexRequest
    {
        public int RowIndex { get; set; }
    }

    public class ResidentGridBatchSummaryRequest
    {
        public FusionGridBatchChanges<ResidentDirectoryGridItem>? BatchChanges { get; set; }
    }

    public sealed record ResidentDirectoryRecord(
        int ResidentId,
        string ResidentName,
        int Age,
        string CareLevel,
        string Wing,
        string Suite,
        string RiskLevel,
        string PrimaryNurse,
        int OpenTasks,
        DateOnly NextReviewDate);
}
