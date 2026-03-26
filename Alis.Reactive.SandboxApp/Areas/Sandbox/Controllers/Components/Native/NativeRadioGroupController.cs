using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeRadioGroup")]
    public class NativeRadioGroupController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.RoomTypeItems = new[]
            {
                new RadioButtonItem("Shared", "Shared Room", "Cost-effective with a roommate"),
                new RadioButtonItem("Private", "Private Room", "Your own space with shared facilities"),
                new RadioButtonItem("Suite", "Private Suite", "Full suite with private bathroom and kitchenette"),
            };

            return View("~/Areas/Sandbox/Views/Components/Native/NativeRadioGroup/Index.cshtml", new NativeRadioGroupModel());
        }

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] NativeRadioGroupModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new { ResidentName = new[] { "Request body is required." } } });

            var validator = new NativeRadioGroupFormValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    var key = failure.PropertyName;
                    if (!errors.ContainsKey(key))
                        errors[key] = new[] { failure.ErrorMessage };
                    else
                    {
                        var existing = errors[key];
                        var extended = new string[existing.Length + 1];
                        existing.CopyTo(extended, 0);
                        extended[existing.Length] = failure.ErrorMessage;
                        errors[key] = extended;
                    }
                }
                return BadRequest(new { errors });
            }

            return Ok(new { message = $"Preferences saved for {model.ResidentName}: {model.RoomType}" });
        }
    }
}
