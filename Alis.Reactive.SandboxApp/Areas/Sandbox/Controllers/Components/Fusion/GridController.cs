using Alis.Reactive.FluentValidator;
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

        [HttpGet("Editing")]
        public IActionResult Editing()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Editing.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("ActionCompleteSaveEdit")]
        public IActionResult ActionCompleteSaveEdit()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ActionCompleteSaveEdit.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("BeginEditNormal")]
        public IActionResult BeginEditNormal()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BeginEditNormal.cshtml",
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

        [HttpGet("CareStaffColumns")]
        public IActionResult CareStaffColumns()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
                new GridOperationsModel());
        }

        [HttpGet("CareReview")]
        public IActionResult CareReview()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
                new GridOperationsModel());
        }

        [HttpGet("ResidentEditForm")]
        public IActionResult ResidentEditForm()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ResidentEditForm.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("BuilderRoster")]
        public IActionResult BuilderRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BuilderRoster.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("RemoteAdaptorRoster")]
        public IActionResult RemoteAdaptorRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/RemoteAdaptorRoster.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("PrintableRoster")]
        public IActionResult PrintableRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/PrintableRoster.cshtml",
                new ResidentGridEditingModel());
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
        /// Response shape is {result, count} for Syncfusion Grid custom binding.
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

        private static IActionResult ValidationError(IReadOnlyDictionary<string, string[]> errors) =>
            new BadRequestObjectResult(new { errors });

        private static bool TryValidate<T>(IValidator<T> validator, T model, out IActionResult error)
        {
            var result = validator.Validate(model);
            error = result.IsValid ? null! : ValidationError(result.ErrorsByField);
            return result.IsValid;
        }
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
