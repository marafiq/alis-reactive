using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SplitButton;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionSplitButton")]
    public sealed class SplitButtonController : Controller
    {
        private const string OnTheChart = "Saved to Margaret Hale's chart.";

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/SplitButton/Index.cshtml",
                new FusionSplitButtonModel { Kind = "Daily care" });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] CareNoteSave note)
        {
            if (string.IsNullOrWhiteSpace(note.Note))
                return Ok(new CareNoteSaved { Message = "Write what happened before saving." });

            return Ok(note.Action switch
            {
                "notify-family" => new CareNoteSaved
                {
                    Saved = true,
                    Message = OnTheChart + " Susan Hale, her daughter, was sent a text.",
                    FinishedActions = ["notify-family"]
                },
                "flag-nurse" => new CareNoteSaved
                {
                    Saved = true,
                    Message = OnTheChart + " Nurse Okafor will review it.",
                    FinishedActions = ["flag-nurse"]
                },
                _ => new CareNoteSaved { Saved = true, Message = OnTheChart }
            });
        }
    }
}
