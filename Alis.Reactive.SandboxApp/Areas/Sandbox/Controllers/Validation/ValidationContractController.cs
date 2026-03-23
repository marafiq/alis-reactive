using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.Contract;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/Contract")]
    public class ValidationContractController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View("~/Areas/Sandbox/Views/Validation/Contract/Index.cshtml", new ResidentModel());
        }

        [HttpGet("ConditionalHide")]
        public IActionResult ConditionalHide()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View("~/Areas/Sandbox/Views/Validation/Contract/ConditionalHide.cshtml", new ResidentModel());
        }

        [HttpGet("ServerPartial")]
        public IActionResult ServerPartial()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View("~/Areas/Sandbox/Views/Validation/Contract/ServerPartial.cshtml", new ResidentModel());
        }

        [HttpGet("AjaxPartial")]
        public IActionResult AjaxPartial()
        {
            ViewBag.CareLevels = new[] { "Independent", "Assisted Living", "Memory Care" };
            return View("~/Areas/Sandbox/Views/Validation/Contract/AjaxPartial.cshtml", new ResidentModel());
        }

        [HttpGet("AddressPartial")]
        public IActionResult AddressPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Validation/Contract/_AddressPartial.cshtml", new ResidentModel());
        }

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] ResidentModel? model)
        {
            return ValidateAndRespond(model, new ResidentValidator());
        }

        [HttpPost("SubmitServerPartial")]
        public IActionResult SubmitServerPartial([FromBody] ResidentModel? model)
        {
            return ValidateAndRespond(model, new ServerPartialValidator());
        }

        [HttpPost("SubmitAjaxPartial")]
        public IActionResult SubmitAjaxPartial([FromBody] ResidentModel? model)
        {
            return ValidateAndRespond(model, new AjaxPartialValidator());
        }

        private IActionResult ValidateAndRespond(ResidentModel? model, FluentValidation.IValidator<ResidentModel> validator)
        {
            if (model == null)
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });

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
