using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionRadioButton")]
    public sealed class RadioButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/RadioButton/Index.cshtml", new FusionRadioButtonModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionRadioButtonEchoRequest request)
        {
            return Ok(new FusionRadioButtonEchoResponse
            {
                SelectedValue = request.SelectedValue,
                PrivateChecked = request.PrivateChecked,
                SharedChecked = request.SharedChecked,
                SharedDisabled = request.SharedDisabled,
                Summary = request.SelectedValue + ":" + request.PrivateChecked + ":" + request.SharedChecked + ":" + request.SharedDisabled
            });
        }
    }
}
