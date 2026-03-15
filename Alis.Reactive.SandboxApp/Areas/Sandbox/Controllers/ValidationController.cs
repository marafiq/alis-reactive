using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using FluentValidation.Results;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ValidationController : Controller
    {
        public IActionResult Index()
        {
            return View(new ValidationShowcaseModel());
        }

        [HttpGet]
        public IActionResult SameModelMerge()
        {
            return View(new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult StandaloneIsolation()
        {
            return View(new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult MultiPartialWorkflow()
        {
            return View(new ValidationMergeHarnessModel());
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

        [HttpPost]
        public IActionResult SaveProfile([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
            {
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Hidden.Name"] = new[] { "Request body is required." } } });
            }

            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(model.Hidden?.Name))
            {
                errors["Hidden.Name"] = new[] { "Name is required." };
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Profile saved! Hidden fields were correctly skipped." });
        }

        [HttpPost]
        public IActionResult SaveWithDbCheck([FromBody] ValidationShowcaseModel? model)
        {
            if (model == null)
            {
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Db.Name"] = new[] { "Request body is required." } } });
            }

            // Validate only the Db section (not the full model)
            var dbSection = model.Db;
            if (dbSection != null)
            {
                var sectionValidator = new BasicSectionValidator();
                var fvResult = sectionValidator.Validate(dbSection);

                if (!fvResult.IsValid)
                {
                    var fvErrors = new Dictionary<string, string[]>();
                    foreach (var failure in fvResult.Errors)
                    {
                        fvErrors["Db." + failure.PropertyName] = new[] { failure.ErrorMessage };
                    }
                    return BadRequest(new { errors = fvErrors });
                }
            }

            var dbErrors = new Dictionary<string, string[]>();

            if (model.Db?.Email != null && model.Db.Email.Contains("taken"))
            {
                dbErrors["Db.Email"] = new[] { "This email address is already registered." };
            }

            if (model.Db?.Name != null && model.Db.Name.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                dbErrors["Db.Name"] = new[] { "This username is reserved by the system." };
            }

            if (dbErrors.Count > 0)
            {
                return BadRequest(new { errors = dbErrors });
            }

            return Ok(new { message = "Saved to database successfully!" });
        }

        [HttpPost]
        public IActionResult ValidateClient() => Ok(new { message = "Client validation passed!" });

        [HttpGet]
        public IActionResult SameModelMergeAddressPartial()
        {
            return PartialView("_SameModelMergeAddressPartial", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult SameModelMergeAddressPartialV2()
        {
            return PartialView("_SameModelMergeAddressPartialV2", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult SameModelMergeDeliveryPartial()
        {
            return PartialView("_SameModelMergeDeliveryPartial", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult IsolationAddressPartial()
        {
            return PartialView("_IsolationAddressPartial", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult IsolationContactPartial()
        {
            return PartialView("_IsolationContactPartial", new ContactFormModel());
        }

        [HttpGet]
        public IActionResult WorkflowAddressPartial()
        {
            return PartialView("_WorkflowAddressPartial", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult WorkflowAddressPartialV2()
        {
            return PartialView("_WorkflowAddressPartialV2", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult WorkflowDeliveryPartial()
        {
            return PartialView("_WorkflowDeliveryPartial", new ValidationMergeHarnessModel());
        }

        [HttpGet]
        public IActionResult WorkflowContactPartial()
        {
            return PartialView("_WorkflowContactPartial", new ContactFormModel());
        }

        [HttpPost]
        public IActionResult SaveSameModelMerge([FromBody] ValidationMergeHarnessModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Root.Name"] = new[] { "Request body is required." }
                    }
                });
            }

            var result = new ValidationMergeHarnessValidator().Validate(model);
            if (!result.IsValid)
            {
                return BuildValidationBadRequest(result);
            }

            return Ok(new { message = "Merged root saved" });
        }

        [HttpPost]
        public IActionResult SaveIsolationParent([FromBody] ValidationMergeHarnessModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Root.Name"] = new[] { "Request body is required." }
                    }
                });
            }

            var result = new ValidationIsolationValidator().Validate(model);
            if (!result.IsValid)
            {
                return BuildValidationBadRequest(result);
            }

            return Ok(new { message = "Parent root saved" });
        }

        [HttpPost]
        public IActionResult SendIsolationContact([FromBody] ContactFormModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Name"] = new[] { "Request body is required." }
                    }
                });
            }

            var result = new ContactFormValidator().Validate(model);
            if (!result.IsValid)
            {
                return BuildValidationBadRequest(result);
            }

            return Ok(new { message = "Standalone contact sent" });
        }

        [HttpPost]
        public IActionResult SaveWorkflowRoot([FromBody] ValidationMergeHarnessModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Root.Name"] = new[] { "Request body is required." }
                    }
                });
            }

            var result = new ValidationMergeHarnessValidator().Validate(model);
            if (!result.IsValid)
            {
                return BuildValidationBadRequest(result);
            }

            return Ok(new { message = "Workflow root saved" });
        }

        [HttpPost]
        public IActionResult SendWorkflowContact([FromBody] ContactFormModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Name"] = new[] { "Request body is required." }
                    }
                });
            }

            var result = new ContactFormValidator().Validate(model);
            if (!result.IsValid)
            {
                return BuildValidationBadRequest(result);
            }

            return Ok(new { message = "Workflow contact sent" });
        }

        [HttpGet]
        public IActionResult ContactFormPartial()
        {
            return PartialView("_ContactFormPartial", new ContactFormModel());
        }

        [HttpPost]
        public IActionResult SendContact([FromBody] ContactFormModel? model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Name"] = new[] { "Name is required." }
                    }
                });
            }

            var validator = new ContactFormValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    errors[failure.PropertyName] = new[] { failure.ErrorMessage };
                }
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Message sent!" });
        }

        [HttpGet]
        public IActionResult AddressPartial()
        {
            return PartialView("_AddressPartial", new ValidationShowcaseModel());
        }

        [HttpGet]
        public IActionResult DeliveryNotePartial()
        {
            return PartialView("_DeliveryNotePartial", new ValidationShowcaseModel());
        }

        [HttpPost]
        public IActionResult SaveDeliveryNote([FromBody] ValidationShowcaseModel? model)
        {
            if (model?.Nested?.Delivery == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Nested.Delivery.Instructions"] = new[] { "Delivery instructions are required." }
                    }
                });
            }

            var validator = new DeliveryNoteValidator();
            var result = validator.Validate(model.Nested.Delivery);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    errors["Nested.Delivery." + failure.PropertyName] = new[] { failure.ErrorMessage };
                }
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Delivery note saved!" });
        }

        [HttpPost]
        public IActionResult SaveAddress([FromBody] ValidationShowcaseModel? model)
        {
            if (model?.Nested?.Address == null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["Nested.Address.Street"] = new[] { "Street is required." },
                        ["Nested.Address.City"] = new[] { "City is required." }
                    }
                });
            }

            var addressValidator = new ValidationAddressValidator();
            var result = addressValidator.Validate(model.Nested.Address);

            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    var key = "Nested.Address." + failure.PropertyName;
                    errors[key] = new[] { failure.ErrorMessage };
                }
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Address saved successfully!" });
        }

        private IActionResult BuildValidationBadRequest(ValidationResult result)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var failure in result.Errors)
            {
                AppendError(errors, failure.PropertyName, failure.ErrorMessage);
            }

            return BadRequest(new { errors });
        }

        private static void AppendError(Dictionary<string, string[]> errors, string key, string message)
        {
            if (!errors.TryGetValue(key, out var existing))
            {
                errors[key] = new[] { message };
                return;
            }

            var extended = new string[existing.Length + 1];
            existing.CopyTo(extended, 0);
            extended[existing.Length] = message;
            errors[key] = extended;
        }
    }
}
