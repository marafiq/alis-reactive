using System.Collections.Generic;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TomSelect")]
    public class TomSelectController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/TomSelect/";

        private static readonly Dictionary<string, Suite> Suites = new()
        {
            ["studio"] = new Suite("Studio", 4200m, "3 studios are open in the North wing."),
            ["one-bedroom"] = new Suite("One Bedroom", 5100m, "1 one-bedroom suite is open in the East wing."),
            ["companion"] = new Suite("Companion Suite", 3600m, "2 companion suites are open in the North wing."),
            ["memory-care"] = new Suite("Memory Care Suite", 6800m, "2 memory care suites are open in the Garden wing."),
        };

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new SuiteAssignmentModel());

        [HttpGet("Rate/{suiteType}")]
        public IActionResult Rate(string suiteType) =>
            Ok(new SuiteRateResponse { SuiteType = suiteType, MonthlyRate = Suites[suiteType].MonthlyRate });

        [HttpGet("Availability/{suiteType}")]
        public IActionResult Availability(string suiteType) =>
            Ok(new SuiteAvailabilityResponse { Summary = Suites[suiteType].Availability });

        // The care assessment recommends memory care for this resident.
        [HttpGet("Recommendation")]
        public IActionResult Recommendation() =>
            Ok(new SuiteRecommendationResponse { SuiteType = "memory-care" });

        [HttpPost("Reserve")]
        public IActionResult Reserve([FromBody] SuiteAssignmentModel assignment)
        {
            if (string.IsNullOrEmpty(assignment.SuiteType) || !Suites.TryGetValue(assignment.SuiteType, out var suite))
                return BadRequest();

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

        private sealed record Suite(string Name, decimal MonthlyRate, string Availability);
    }
}
