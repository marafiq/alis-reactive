using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Margaret Hale's change-of-condition page: the physician directory the nurse searches, how each
    /// physician is reached, who is on call tonight, and the page that goes out.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/AutoComplete")]
    public sealed class AutoCompleteController : Controller
    {
        // The directory answers after a moment, as the hospital's directory service does; the page says it is
        // searching while it waits.
        private static readonly TimeSpan DirectoryLookup = TimeSpan.FromMilliseconds(1500);

        private static readonly IReadOnlyList<DirectoryEntry> Physicians =
        [
            new("dr-patel", "Dr. Anita Patel", "Geriatrics", "555-0142"),
            new("dr-patterson", "Dr. Lena Patterson", "Internal Medicine", "555-0161"),
            new("dr-okafor", "Dr. James Okafor", "Cardiology", "555-0148"),
            new("dr-reyes", "Dr. Maria Reyes", "Hospital Medicine", "555-0177"),
            new("dr-lim", "Dr. Wen Lim", "Psychiatry", "555-0183"),
            new("dr-novak", "Dr. Elena Novak", "Nephrology", "555-0190")
        ];

        private static readonly string[] CareTeam = ["dr-patel", "dr-okafor"];

        private static readonly string[] OnCallTonight = ["dr-reyes", "dr-lim"];

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml", new FusionAutoCompleteModel
            {
                CareTeam = Listings(CareTeam),
                Attending = CareTeam[0]
            });

        [HttpGet("Directory")]
        public async Task<IActionResult> Search([FromQuery] DirectorySearch search)
        {
            await Task.Delay(DirectoryLookup);
            var typed = search.Name?.Trim() ?? "";
            var matches = Physicians
                .Where(entry => typed.Length > 0 && entry.Name.Contains(typed, StringComparison.OrdinalIgnoreCase))
                .Select(Listing)
                .ToList();
            return Ok(new DirectoryMatches
            {
                Physicians = matches,
                Count = matches.Count,
                Summary = (typed.Length, matches.Count) switch
                {
                    (0, _) => "Type part of a name to search the physician directory.",
                    (_, 0) => $"No physician in the directory matches \"{typed}\".",
                    (_, 1) => $"1 physician in the directory matches \"{typed}\".",
                    _ => $"{matches.Count} physicians in the directory match \"{typed}\"."
                }
            });
        }

        [HttpGet("Contact")]
        public IActionResult Contact([FromQuery] PhysicianContactQuery query)
        {
            var entry = Find(query.Physician);
            return entry is null
                ? NotFound()
                : Ok(new PhysicianContact { Specialty = entry.Specialty, Callback = entry.Callback });
        }

        [HttpGet("OnCall")]
        public IActionResult OnCall() => Ok(new OnCallPhysicians { Physicians = Listings(OnCallTonight) });

        [HttpPost("Send")]
        public IActionResult Send([FromBody] FusionAutoCompleteModel? report)
        {
            if (report == null) return BadRequest();

            var result = new PhysicianPageValidator().Validate(report);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var physician = Find(report.Physician);
            if (physician is null)
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Physician"] = ["That physician is not in the directory."] } });

            return Ok(new PhysicianPageSent
            {
                Message = $"Paged {physician.Name} on {physician.Callback} about Margaret Hale: {report.Concern!.Trim()}"
            });
        }

        private static DirectoryEntry? Find(string? id) => Physicians.FirstOrDefault(entry => entry.Id == id);

        private static List<PhysicianListing> Listings(IEnumerable<string> ids) =>
            ids.Select(id => Listing(Find(id)!)).ToList();

        private static PhysicianListing Listing(DirectoryEntry entry) => new() { Id = entry.Id, Name = entry.Name };

        private sealed record DirectoryEntry(string Id, string Name, string Specialty, string Callback);
    }
}
