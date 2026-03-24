using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/Http")]
    public class HttpController : Controller
    {
        private static readonly object NativeActionLinkLock = new object();
        private static readonly HttpActionLinkRow[] NativeActionLinkSeedRows =
        {
            new HttpActionLinkRow { Id = 41, Name = "John Doe" },
            new HttpActionLinkRow { Id = 42, Name = "Jane Smith" },
            new HttpActionLinkRow { Id = 43, Name = "Bob Johnson" }
        };
        private static HashSet<int> _deletedNativeActionLinkRows = new HashSet<int>();

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ResetNativeActionLinkRows();

            var model = new HttpShowcaseModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ActionRows = GetNativeActionLinkRows()
            };
            return View("~/Areas/Sandbox/Views/HttpPipeline/Http/Index.cshtml", model);
        }

        // ── Section 1: DomReady GET ──────────────────────────

        [HttpGet("Residents")]
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

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errorSummary = "Validation failed: Name is required" });
            }

            return Ok(new { message = $"Saved: {request.Name}", receivedName = request.Name });
        }

        // ── Section 3: Chained GET ───────────────────────────

        [HttpGet("Facilities")]
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

        [HttpPut("UpdateResident")]
        public IActionResult UpdateResident([FromBody] UpdateRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errors = new { Name = new[] { "Name is required." } } });
            }

            return Ok(new { receivedName = request.Name, receivedFacilityId = request.FacilityId?.ToString() ?? "", updated = true });
        }

        // ── Section 6: DELETE ────────────────────────────────

        [HttpDelete("DeleteResident/{id}")]
        public IActionResult DeleteResident(int id)
        {
            return Ok(new { deleted = true, deletedId = id });
        }

        // ── Section 7: POST FormData ─────────────────────────

        [HttpPost("SaveFormData")]
        public IActionResult SaveFormData([FromBody] SaveFormRequest? request)
        {
            var fields = new List<string>();
            if (!string.IsNullOrEmpty(request?.FirstName)) fields.Add("FirstName");
            if (!string.IsNullOrEmpty(request?.LastName)) fields.Add("LastName");
            if (!string.IsNullOrEmpty(request?.Email)) fields.Add("Email");
            return Ok(new { receivedFields = string.Join(", ", fields), count = fields.Count });
        }

        // ── Section 8: GET search ────────────────────────────

        [HttpGet("Search")]
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

        [HttpPost("ValidateResident")]
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

        [HttpGet("NativeActionLinkGrid")]
        public IActionResult NativeActionLinkGrid()
        {
            return PartialView("~/Areas/Sandbox/Views/HttpPipeline/Http/_NativeActionLinkGrid.cshtml", GetNativeActionLinkRows());
        }

        // ── Section 10: NativeActionLink row action ───────────

        [HttpPost("ActionLinkDelete/{id}")]
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

        [HttpPost("StandaloneNativeActionLink")]
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

        private static IReadOnlyList<HttpActionLinkRow> GetNativeActionLinkRows()
        {
            lock (NativeActionLinkLock)
            {
                return NativeActionLinkSeedRows
                    .Where(row => !_deletedNativeActionLinkRows.Contains(row.Id))
                    .Select(row => new HttpActionLinkRow
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
