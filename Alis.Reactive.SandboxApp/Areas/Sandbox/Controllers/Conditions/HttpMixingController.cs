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

        // ── Section 1: POST echo — returns posted name ──

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            return Ok(new { receivedName = request?.Name ?? "", saved = true });
        }

        // ── Section 2: POST audit — returns audit result ──

        [HttpPost("Audit")]
        public IActionResult Audit([FromBody] AuditRequest? request)
        {
            return Ok(new { result = $"audited:{request?.Action ?? "unknown"}" });
        }

        // ── Section 3: POST classify — returns tier based on count ──

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

        // ── Section 4: POST that returns 400 — for OnError condition testing ──

        [HttpPost("FailValidation")]
        public IActionResult FailValidation()
        {
            return BadRequest(new { errorSummary = "Name is required" });
        }

        // ── DTOs ──

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
