using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeHiddenField")]
    public class NativeHiddenFieldController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/NativeHiddenField/Index.cshtml", new NativeHiddenFieldModel
            {
                ResidentId = "RES-1042",
                FormToken = "abc123"
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] NativeHiddenFieldModel? model)
        {
            if (model == null)
                return BadRequest(new { error = "Empty body" });

            var count = 0;
            if (model.ResidentId != null) count++;
            if (model.FormToken != null) count++;
            if (model.ResidentName != null) count++;

            return Ok(new
            {
                residentId = model.ResidentId,
                formToken = model.FormToken,
                residentName = model.ResidentName,
                fieldCount = count
            });
        }
    }
}
