using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class HttpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // ── Section 1: DomReady GET ──────────────────────────

        [HttpGet]
        public IActionResult Residents()
        {
            return Json(new[]
            {
                new { name = "John Doe", age = 82 },
                new { name = "Jane Smith", age = 75 }
            });
        }

        // ── Section 2: POST with gather ──────────────────────

        [HttpPost]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errors = new { Name = new[] { "Name is required." } } });
            }

            return Ok(new { message = $"Saved: {request.Name}" });
        }

        // ── Section 3: Chained GET ───────────────────────────

        [HttpGet]
        public IActionResult Facilities()
        {
            return Json(new[]
            {
                new { id = 1, name = "Main Campus" },
                new { id = 2, name = "West Wing" }
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

            return Ok(new { name = request.Name, facilityId = request.FacilityId, updated = true });
        }

        // ── Section 6: DELETE ────────────────────────────────

        [HttpDelete]
        public IActionResult DeleteResident(int id)
        {
            return Ok(new { deleted = true, id });
        }

        // ── Section 7: POST FormData ─────────────────────────

        [HttpPost]
        public IActionResult SaveFormData([FromForm] SaveFormRequest? request)
        {
            var fields = new List<string>();
            if (!string.IsNullOrEmpty(request?.FirstName)) fields.Add("FirstName");
            if (!string.IsNullOrEmpty(request?.LastName)) fields.Add("LastName");
            if (!string.IsNullOrEmpty(request?.Email)) fields.Add("Email");
            return Ok(new { receivedFields = fields, count = fields.Count });
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

            if (string.IsNullOrEmpty(q)) return Json(all);
            return Json(all.Where(r => r.name.Contains(q, StringComparison.OrdinalIgnoreCase)));
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
                return UnprocessableEntity(new { errors });

            return Ok(new { valid = true });
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
    }
}
