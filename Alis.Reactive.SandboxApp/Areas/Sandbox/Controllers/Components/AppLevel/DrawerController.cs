using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Drawer")]
    public partial class DrawerController : Controller
    {
        [HttpGet("")]
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
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new { message = $"Resident {model.Name} added successfully" });
        }
    }
}
