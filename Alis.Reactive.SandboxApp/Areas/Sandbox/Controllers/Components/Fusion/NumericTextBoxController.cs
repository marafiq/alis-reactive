using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.NumericTextBox;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NumericTextBox")]
    public class NumericTextBoxController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml", new NumericTextBoxModel
            {
                Amount = 0,
                Temperature = 0,
                Quantity = 1
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
