using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.HttpMixing;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions
{
    [Area("Sandbox")]
    [Route("Sandbox/Conditions/HttpMixing")]
    public class HttpMixingController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Conditions/HttpMixing/Index.cshtml", new HttpMixingModel());
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            return Ok(new { receivedName = request?.Name ?? "", saved = true });
        }

        [HttpPost("Audit")]
        public IActionResult Audit([FromBody] AuditRequest? request)
        {
            return Ok(new { result = $"audited:{request?.Action ?? "unknown"}" });
        }

        [HttpPost("Resident/{residentId:int}/Audit")]
        public IActionResult AuditResident(
            int residentId,
            [FromBody] AuditRequest? request,
            [FromHeader(Name = "X-Category")] string? category)
        {
            return Ok(new
            {
                residentId,
                action = request?.Action ?? "unknown",
                headerCategory = category ?? ""
            });
        }

        [HttpPost("Resident/{residentId:int}/AuditTrail/{categorySlug}")]
        public IActionResult AuditResidentTrail(int residentId, string categorySlug)
        {
            return Ok(new
            {
                residentId,
                categorySlug,
                step = "chained"
            });
        }

        [HttpPost("Classify")]
        public IActionResult Classify([FromBody] ClassifyRequest? request)
        {
            var tier = request?.Count switch
            {
                > 100 => "enterprise",
                > 50 => "business",
                > 10 => "team",
                _ => "individual"
            };
            return Ok(new { tier });
        }

        [HttpPost("FailValidation")]
        public IActionResult FailValidation()
        {
            return BadRequest(new { errorSummary = "Name is required" });
        }

        public class SaveRequest
        {
            public string? Name { get; set; }
        }

        public class AuditRequest
        {
            public string? Action { get; set; }
        }

        public class ClassifyRequest
        {
            public int Count { get; set; }
        }
    }
}
