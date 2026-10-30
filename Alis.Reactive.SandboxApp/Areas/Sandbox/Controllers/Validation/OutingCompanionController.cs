using Alis.Reactive.FluentValidator;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Outings: the companion is a partial the coordinator adds to the form and removes from it. The
    /// server checks the outing with the companion's rules whenever a companion was sent.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/OutingCompanion")]
    public class OutingCompanionController : Controller
    {
        private const string Views = "~/Areas/Sandbox/Views/Validation/OutingCompanion/";

        [HttpGet("")]
        public IActionResult Index() => View(Views + "Index.cshtml", new OutingModel());

        [HttpGet("Companion")]
        public IActionResult Companion() =>
            PartialView(Views + "_Companion.cshtml", new OutingModel { Companion = new OutingCompanion() });

        [HttpGet("NoCompanion")]
        public IActionResult NoCompanion() => PartialView(Views + "_NoCompanion.cshtml");

        [HttpPost("Save")]
        public IActionResult Save([FromBody] OutingModel? outing)
        {
            if (outing == null) return BadRequest();

            var rules = new InlineValidator<OutingModel>();
            rules.Include(new OutingValidator());
            rules.Include(new OutingCompanionRules());
            var result = rules.Validate(outing);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var companion = outing.Companion == null ? "" : $" with {outing.Companion.Name}";
            return Ok(new OutingConfirmation($"Outing to {outing.Destination} saved for {outing.ResidentName}{companion}."));
        }

        [HttpPost("ConfirmCompanion")]
        public IActionResult ConfirmCompanion([FromBody] OutingModel? outing)
        {
            if (outing == null) return BadRequest();

            var result = new OutingCompanionRules().Validate(outing);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new OutingConfirmation($"{outing.Companion?.Name} confirmed for the outing."));
        }
    }
}
