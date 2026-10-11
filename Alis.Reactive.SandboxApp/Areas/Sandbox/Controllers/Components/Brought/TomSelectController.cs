using System;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TomSelect")]
    public class TomSelectController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/TomSelect/";

        private static readonly Suite[] Suites =
        [
            new Suite("studio", "Studio", 4200m, "3 studios are open in the North wing."),
            new Suite("one-bedroom", "One Bedroom", 5100m, "1 one-bedroom suite is open in the East wing."),
            new Suite("companion", "Companion Suite", 3600m, "2 companion suites are open in the North wing."),
            new Suite("memory-care", "Memory Care Suite", 6800m, "2 memory care suites are open in the Garden wing."),
        ];

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new SuiteAssignmentModel());

        [HttpGet("Rate/{suiteType}")]
        public IActionResult Rate(string suiteType)
        {
            var suite = Find(suiteType);
            if (suite == null) return NotFound();

            return Ok(new SuiteRateResponse { SuiteType = suite.Type, MonthlyRate = suite.MonthlyRate });
        }

        [HttpGet("Availability/{suiteType}")]
        public IActionResult Availability(string suiteType)
        {
            var suite = Find(suiteType);
            if (suite == null) return NotFound();

            return Ok(new SuiteAvailabilityResponse { Summary = suite.Availability });
        }

        // The care assessment recommends memory care for this resident.
        [HttpGet("Recommendation")]
        public IActionResult Recommendation() =>
            Ok(new SuiteRecommendationResponse { SuiteType = "memory-care" });

        [HttpPost("Reserve")]
        public IActionResult Reserve([FromBody] SuiteAssignmentModel assignment)
        {
            var suite = Find(assignment.SuiteType);
            if (suite == null) return BadRequest();

            return Ok(new SuiteReservationResponse
            {
                Confirmation = suite.Name + " reserved at $" + suite.MonthlyRate.ToString("N0") + " a month.",
            });
        }

        [HttpGet("WelcomePacket")]
        public IActionResult WelcomePacket() =>
            Ok(new MoveInDocumentResponse { Title = "Welcome packet prepared." });

        [HttpGet("MoveInChecklist")]
        public IActionResult MoveInChecklist() =>
            Ok(new MoveInDocumentResponse { Title = "Move-in checklist prepared." });

        [HttpGet("Companion")]
        public IActionResult Companion() =>
            PartialView(ViewBase + "_CompanionSuite.cshtml", new CompanionSuiteModel());

        [HttpGet("NoCompanion")]
        public IActionResult NoCompanion() => PartialView(ViewBase + "_NoCompanion.cshtml");

        private static Suite? Find(string? suiteType) => Array.Find(Suites, suite => suite.Type == suiteType);

        private sealed record Suite(string Type, string Name, decimal MonthlyRate, string Availability);
    }
}
