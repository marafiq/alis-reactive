using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ValidationContractController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View(new ResidentModel());
        }

        public IActionResult ConditionalHide()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View(new ResidentModel());
        }

        public IActionResult ServerPartial()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View(new ResidentModel());
        }

        public IActionResult AjaxPartial()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View(new ResidentModel());
        }

        [HttpGet]
        public IActionResult AddressPartial()
        {
            return PartialView("_AddressPartial", new ResidentModel());
        }

        [HttpPost]
        public IActionResult Submit([FromBody] ResidentModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });

            var validator = new ResidentValidator();
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

            return Ok(new { message = "Admission saved" });
        }
    }
}
