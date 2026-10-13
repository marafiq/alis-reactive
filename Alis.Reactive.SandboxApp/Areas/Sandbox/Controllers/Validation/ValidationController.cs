using Alis.Reactive.FluentValidator;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/AllRules")]
    public class ValidationController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Validation/AllRules/Index.cshtml", new ValidationShowcaseModel
            {
                Lines =
                {
                    new ValidationOrderLine
                    {
                        Deliveries = { new ValidationLineDelivery() }
                    },
                    new ValidationOrderLine
                    {
                        Deliveries = { new ValidationLineDelivery() }
                    }
                }
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });

            var result = new ValidationShowcaseValidator().Validate(model);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

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
                // The section's own rules name its fields from the section; the page names them under Db.
                var sectionResult = new BasicSectionValidator().Validate(dbSection);
                if (!sectionResult.IsValid)
                {
                    var sectionErrors = sectionResult.ErrorsByField.ToDictionary(field => "Db." + field.Key, field => field.Value);
                    return BadRequest(new { errors = sectionErrors });
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
    }
}
