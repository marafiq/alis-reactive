using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ValidationController : Controller
    {
        private static readonly FluentValidationAdapter _adapter = new FluentValidationAdapter();

        public IActionResult Index()
        {
            // Extract client-side validation rules from the FluentValidation validator
            var descriptor = _adapter.ExtractRules(
                typeof(ValidationShowcaseValidator), "validation-form");

            ViewBag.ValidationDescriptor = descriptor;
            return View(new ValidationShowcaseModel());
        }

        [HttpPost]
        public IActionResult Save([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
            {
                return BadRequest(new { errors = new { Name = new[] { "Request body is required." } } });
            }

            var validator = new ValidationShowcaseValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    var key = failure.PropertyName;
                    if (!errors.ContainsKey(key))
                    {
                        errors[key] = new[] { failure.ErrorMessage };
                    }
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

            return Ok(new { message = "Saved successfully!" });
        }

        /// <summary>
        /// POST endpoint simulating hidden-fields form. Server validates only the visible fields.
        /// Always accepts since hidden fields are excluded from client-side validation.
        /// </summary>
        [HttpPost]
        public IActionResult SaveProfile([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
            {
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Name"] = new[] { "Request body is required." } } });
            }

            // Only validate Name (the always-visible required field)
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors["Name"] = new[] { "Name is required." };
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Profile saved! Hidden fields were correctly skipped." });
        }

        /// <summary>
        /// POST endpoint simulating fake database validation.
        /// Checks for "duplicate" email and "reserved" usernames — returns 400 ProblemDetails.
        /// </summary>
        [HttpPost]
        public IActionResult SaveWithDbCheck([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
            {
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Name"] = new[] { "Request body is required." } } });
            }

            // First run FluentValidation
            var validator = new ValidationShowcaseValidator();
            var fvResult = validator.Validate(model);

            if (!fvResult.IsValid)
            {
                var fvErrors = new Dictionary<string, string[]>();
                foreach (var failure in fvResult.Errors)
                {
                    fvErrors[failure.PropertyName] = new[] { failure.ErrorMessage };
                }
                return BadRequest(new { errors = fvErrors });
            }

            // Simulate database-level checks
            var dbErrors = new Dictionary<string, string[]>();

            if (model.Email != null && model.Email.Contains("taken"))
            {
                dbErrors["Email"] = new[] { "This email address is already registered." };
            }

            if (model.Name != null && model.Name.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                dbErrors["Name"] = new[] { "This username is reserved by the system." };
            }

            if (dbErrors.Count > 0)
            {
                return BadRequest(new { errors = dbErrors });
            }

            return Ok(new { message = "Saved to database successfully!" });
        }

        /// <summary>
        /// Returns a partial view with address fields — simulates dynamic form sections
        /// loaded after the initial page render (e.g. "Add Address" button).
        /// </summary>
        [HttpGet]
        public IActionResult AddressPartial()
        {
            return PartialView("_AddressPartial");
        }

        /// <summary>
        /// POST endpoint that validates only the address portion.
        /// Returns 400 with nested field errors (Address.Street, etc.) or 200.
        /// </summary>
        [HttpPost]
        public IActionResult SaveAddress([FromBody] ValidationShowcaseModel? model)
        {
            if (model?.Address == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Address.Street"] = new[] { "Street is required." },
                        ["Address.City"] = new[] { "City is required." }
                    }
                });
            }

            var addressValidator = new ValidationAddressValidator();
            var result = addressValidator.Validate(model.Address);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    var key = "Address." + failure.PropertyName;
                    errors[key] = new[] { failure.ErrorMessage };
                }
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Address saved successfully!" });
        }
    }
}
