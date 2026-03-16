using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class HttpController : Controller
    {
        private static readonly object NativeActionLinkLock = new object();
        private static readonly Models.HttpActionLinkRow[] NativeActionLinkSeedRows =
        {
            new Models.HttpActionLinkRow { Id = 41, Name = "John Doe" },
            new Models.HttpActionLinkRow { Id = 42, Name = "Jane Smith" },
            new Models.HttpActionLinkRow { Id = 43, Name = "Bob Johnson" }
        };
        private static HashSet<int> _deletedNativeActionLinkRows = new HashSet<int>();

        public IActionResult Index()
        {
            ResetNativeActionLinkRows();

            var model = new Models.HttpShowcaseModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ActionRows = GetNativeActionLinkRows()
            };
            return View(model);
        }

        // ── Section 1: DomReady GET ──────────────────────────

        [HttpGet]
        public IActionResult Residents()
        {
            return Json(new
            {
                first = "John Doe",
                second = "Jane Smith",
                count = 2
            });
        }

        // ── Section 2: POST with gather ──────────────────────

        [HttpPost]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errorSummary = "Validation failed: Name is required" });
            }

            return Ok(new { message = $"Saved: {request.Name}", receivedName = request.Name });
        }

        // ── Section 3: Chained GET ───────────────────────────

        [HttpGet]
        public IActionResult Facilities()
        {
            return Json(new
            {
                first = "Main Campus",
                second = "West Wing",
                count = 2
            });
        }

        // ── Section 5: PUT update ────────────────────────────

        [HttpPut]
        public IActionResult UpdateResident([FromBody] UpdateRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errors = new { Name = new[] { "Name is required." } } });
            }

            return Ok(new { receivedName = request.Name, receivedFacilityId = request.FacilityId?.ToString() ?? "", updated = true });
        }

        // ── Section 6: DELETE ────────────────────────────────

        [HttpDelete]
        public IActionResult DeleteResident(int id)
        {
            return Ok(new { deleted = true, deletedId = id });
        }

        // ── Section 7: POST FormData ─────────────────────────

        [HttpPost]
        public IActionResult SaveFormData([FromBody] SaveFormRequest? request)
        {
            var fields = new List<string>();
            if (!string.IsNullOrEmpty(request?.FirstName)) fields.Add("FirstName");
            if (!string.IsNullOrEmpty(request?.LastName)) fields.Add("LastName");
            if (!string.IsNullOrEmpty(request?.Email)) fields.Add("Email");
            return Ok(new { receivedFields = string.Join(", ", fields), count = fields.Count });
        }

        // ── Section 8: GET search ────────────────────────────

        [HttpGet]
        public IActionResult Search(string? q)
        {
            var all = new[]
            {
                new { name = "John Doe", age = 82 },
                new { name = "Jane Smith", age = 75 },
                new { name = "Bob Johnson", age = 68 }
            };

            var results = string.IsNullOrEmpty(q) ? all : all.Where(r => r.name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToArray();
            return Json(new { query = q ?? "", matchCount = results.Length });
        }

        // ── Section 9: Multi-status validation ───────────────

        [HttpPost]
        public IActionResult ValidateResident([FromBody] ValidateRequest? request)
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request?.Name))
                errors["Name"] = new[] { "Name is required." };
            if (!request?.FacilityId.HasValue ?? true)
                errors["FacilityId"] = new[] { "Facility is required." };

            if (errors.Count > 0)
                return UnprocessableEntity(new { errorSummary = $"422 — {errors.Count} validation error(s): {string.Join(", ", errors.Keys)}" });

            return Ok(new { valid = true });
        }

        [HttpGet]
        public IActionResult NativeActionLinkGrid()
        {
            return PartialView("_NativeActionLinkGrid", GetNativeActionLinkRows());
        }

        // ── Section 10: NativeActionLink row action ───────────

        [HttpPost]
        public IActionResult ActionLinkDelete(int id, [FromBody] ActionLinkDeleteRequest? request)
        {
            if (request?.Id != id)
            {
                return BadRequest(new { errors = new { Id = new[] { "Route id and payload id must match." } } });
            }

            lock (NativeActionLinkLock)
            {
                _deletedNativeActionLinkRows.Add(id);
            }

            return Ok(new { deleted = true, id });
        }

        [HttpPost]
        public IActionResult StandaloneNativeActionLink([FromBody] StandaloneNativeActionLinkRequest? request)
        {
            if (!string.Equals(request?.Command, "run", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { errors = new { Command = new[] { "Command must be 'run'." } } });
            }

            return Content(
                "<div class=\"rounded-md border border-blue-200 bg-blue-50 px-3 py-2 text-sm text-blue-700\">Standalone NativeActionLink response loaded.</div>",
                "text/html");
        }

        private static IReadOnlyList<Models.HttpActionLinkRow> GetNativeActionLinkRows()
        {
            lock (NativeActionLinkLock)
            {
                return NativeActionLinkSeedRows
                    .Where(row => !_deletedNativeActionLinkRows.Contains(row.Id))
                    .Select(row => new Models.HttpActionLinkRow
                    {
                        Id = row.Id,
                        Name = row.Name
                    })
                    .ToArray();
            }
        }

        private static void ResetNativeActionLinkRows()
        {
            lock (NativeActionLinkLock)
            {
                _deletedNativeActionLinkRows = new HashSet<int>();
            }
        }

        // ── DTOs ─────────────────────────────────────────────

        public class SaveRequest
        {
            public string? Name { get; set; }
        }

        public class UpdateRequest
        {
            public string? Name { get; set; }
            public int? FacilityId { get; set; }
        }

        public class SaveFormRequest
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
        }

        public class ValidateRequest
        {
            public string? Name { get; set; }
            public int? FacilityId { get; set; }
        }

        public class ActionLinkDeleteRequest
        {
            public int Id { get; set; }
        }

        public class StandaloneNativeActionLinkRequest
        {
            public string? Command { get; set; }
        }
    }
}
