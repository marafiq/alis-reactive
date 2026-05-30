using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionComboBox")]
    public sealed class ComboBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = new List<ComboBoxResidentItem>
            {
                new() { Value = "alice", Text = "Alice", Unit = "A" },
                new() { Value = "bennett", Text = "Bennett", Unit = "A" },
                new() { Value = "carey", Text = "Carey", Unit = "B" },
                new() { Value = "dawson", Text = "Dawson", Unit = "B" }
            };

            return View("~/Areas/Sandbox/Views/Components/Fusion/ComboBox/Index.cshtml", new FusionComboBoxModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionComboBoxEchoRequest request)
        {
            return Ok(new FusionComboBoxEchoResponse
            {
                Value = request.Value,
                Text = request.Text,
                Index = request.Index,
                Summary = request.Value + ":" + request.Text + ":" + request.Index
            });
        }
    }
}
