using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionListBox")]
    public sealed class ListBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = new List<ListBoxResidentItem>
            {
                new() { Value = "alice", Text = "Alice", Unit = "A Wing" },
                new() { Value = "bennett", Text = "Bennett", Unit = "A Wing" },
                new() { Value = "carey", Text = "Carey", Unit = "B Wing" },
                new() { Value = "dawson", Text = "Dawson", Unit = "B Wing" }
            };

            return View("~/Areas/Sandbox/Views/Components/Fusion/ListBox/Index.cshtml", new FusionListBoxModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionListBoxEchoRequest request)
        {
            var values = request.Value ?? [];
            return Ok(new FusionListBoxEchoResponse
            {
                ValueSummary = string.Join(",", values),
                CountSummary = values.Length.ToString()
            });
        }
    }
}
