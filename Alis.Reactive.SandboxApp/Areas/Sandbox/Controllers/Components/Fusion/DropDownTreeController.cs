using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionDropDownTree")]
    public sealed class DropDownTreeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = new List<DropDownTreeResidentNode>
            {
                new() { Value = "floor-a", Text = "Floor A", HasChildren = true, Expanded = true },
                new() { Value = "alice", Text = "Alice", ParentValue = "floor-a" },
                new() { Value = "bennett", Text = "Bennett", ParentValue = "floor-a" },
                new() { Value = "floor-b", Text = "Floor B", HasChildren = true, Expanded = true },
                new() { Value = "carey", Text = "Carey", ParentValue = "floor-b" },
                new() { Value = "dawson", Text = "Dawson", ParentValue = "floor-b" }
            };

            return View("~/Areas/Sandbox/Views/Components/Fusion/DropDownTree/Index.cshtml", new FusionDropDownTreeModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionDropDownTreeEchoRequest request)
        {
            var valueSummary = string.Join(",", request.Value ?? []);
            return Ok(new FusionDropDownTreeEchoResponse
            {
                ValueSummary = valueSummary,
                Text = request.Text,
                Summary = valueSummary + ":" + request.Text
            });
        }
    }
}
