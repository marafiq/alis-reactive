using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.AllRules;
using FluentValidation.Results;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/AllRules")]
    public class ValidationController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Validation/AllRules/Index.cshtml", new ValidationShowcaseModel());
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });

            var validator = new ValidationShowcaseValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
                return BuildValidationBadRequest(result);

            return Ok(new { message = "Saved successfully!" });
        }

        [HttpPost("SaveProfile")]
        public IActionResult SaveProfile([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Hidden.Name"] = new[] { "Request body is required." } } });

            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(model.Hidden?.Name))
                errors["Hidden.Name"] = new[] { "Name is required." };

            if (errors.Count > 0)
                return BadRequest(new { errors });

            return Ok(new { message = "Profile saved!" });
        }

        [HttpPost("SaveWithDbCheck")]
        public IActionResult SaveWithDbCheck([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Db.Name"] = new[] { "Request body is required." } } });

            var dbSection = model.Db;
            if (dbSection != null)
            {
                var sectionValidator = new BasicSectionValidator();
                var fvResult = sectionValidator.Validate(dbSection);

                if (!fvResult.IsValid)
                {
                    var fvErrors = new Dictionary<string, string[]>();
                    foreach (var failure in fvResult.Errors)
                        fvErrors["Db." + failure.PropertyName] = new[] { failure.ErrorMessage };
                    return BadRequest(new { errors = fvErrors });
                }
            }

            var dbErrors = new Dictionary<string, string[]>();
            if (model.Db?.Email != null && model.Db.Email.Contains("taken"))
                dbErrors["Db.Email"] = new[] { "This email address is already registered." };
            if (model.Db?.Name != null && model.Db.Name.Equals("admin", StringComparison.OrdinalIgnoreCase))
                dbErrors["Db.Name"] = new[] { "This username is reserved by the system." };

            if (dbErrors.Count > 0)
                return BadRequest(new { errors = dbErrors });

            return Ok(new { message = "Saved to database successfully!" });
        }

        [HttpPost("ValidateClient")]
        public IActionResult ValidateClient() => Ok(new { message = "Client validation passed!" });

        private IActionResult BuildValidationBadRequest(ValidationResult result)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var existing))
                {
                    errors[failure.PropertyName] = new[] { failure.ErrorMessage };
                }
                else
                {
                    var extended = new string[existing.Length + 1];
                    existing.CopyTo(extended, 0);
                    extended[existing.Length] = failure.ErrorMessage;
                    errors[failure.PropertyName] = extended;
                }
            }
            return BadRequest(new { errors });
        }
    }
}
