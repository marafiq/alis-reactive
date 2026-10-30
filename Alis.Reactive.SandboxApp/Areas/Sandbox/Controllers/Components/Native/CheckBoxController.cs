using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Shower log: Dorothy Reyes's evening shower, opened with the shower chair her care plan calls for
    /// and saved as the care aide ticked it.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/CheckBox")]
    public class CheckBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/CheckBox/Index.cshtml", new ShowerLog { UsesShowerChair = true });
        }

        [HttpPost("ShowerLog/Save")]
        public IActionResult SaveShowerLog([FromBody] ShowerLog? log)
        {
            if (log == null) return BadRequest();

            var result = new ShowerLogValidator().Validate(log);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var skinCheck = log.SkinConcern ? "; skin concern reported to the nurse" : "";
            return Ok(new ShowerLogSaved { Summary = $"Shower log saved for Dorothy Reyes: {Shower(log)}{skinCheck}." });
        }

        private static string Shower(ShowerLog log)
        {
            if (log.ShowerDeclined) return "shower declined, offer it again before 9 pm";
            return log.UsesShowerChair ? "shower given with the shower chair" : "shower given";
        }
    }
}
