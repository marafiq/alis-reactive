using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class DrawerController : Controller
    {
        public IActionResult Index()
        {
            return View(new DrawerModel());
        }

        [HttpGet]
        public IActionResult ResidentDetails()
        {
            return PartialView("_ResidentDetailsPartial");
        }

        [HttpGet]
        public IActionResult CarePlanNotes()
        {
            return PartialView("_CarePlanNotesPartial");
        }

        [HttpGet]
        public IActionResult AddResidentForm()
        {
            return PartialView("_AddResidentFormPartial", new DrawerResidentModel());
        }

        [HttpPost]
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
