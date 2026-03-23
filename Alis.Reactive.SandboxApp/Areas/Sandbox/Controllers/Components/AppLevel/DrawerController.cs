using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.Drawer;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Drawer")]
    public class DrawerController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/Index.cshtml", new DrawerModel());
        }

        [HttpGet("ResidentDetails")]
        public IActionResult ResidentDetails()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_ResidentDetailsPartial.cshtml");
        }

        [HttpGet("CarePlanNotes")]
        public IActionResult CarePlanNotes()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_CarePlanNotesPartial.cshtml");
        }

        [HttpGet("AddResidentForm")]
        public IActionResult AddResidentForm()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_AddResidentFormPartial.cshtml", new DrawerResidentModel());
        }

        [HttpPost("SubmitResident")]
        public async Task<IActionResult> SubmitResident([FromBody] DrawerResidentModel? model)
        {
            await Task.Delay(5000);
            if (model == null)
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });

            var validator = new DrawerResidentValidator();
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

            return Ok(new { message = $"Resident {model.Name} added successfully" });
        }
    }
}
