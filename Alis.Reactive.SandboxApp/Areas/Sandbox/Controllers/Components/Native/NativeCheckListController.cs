using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeCheckList")]
    public class NativeCheckListController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.AllergyItems = new[]
            {
                new RadioButtonItem("Peanuts", "Peanuts", "Tree nut allergy — avoid all nut products"),
                new RadioButtonItem("Shellfish", "Shellfish", "Crustacean allergy — shrimp, crab, lobster"),
                new RadioButtonItem("Dairy", "Dairy", "Lactose intolerance — milk, cheese, yogurt"),
                new RadioButtonItem("Gluten", "Gluten", "Celiac disease — wheat, barley, rye"),
            };

            ViewBag.AmenityItems = new[]
            {
                new RadioButtonItem("WiFi", "WiFi"),
                new RadioButtonItem("Parking", "Parking"),
                new RadioButtonItem("Laundry", "Laundry"),
                new RadioButtonItem("Gym", "Gym"),
                new RadioButtonItem("Pool", "Pool"),
            };

            ViewBag.DietaryItems = new[]
            {
                new RadioButtonItem("LowSodium", "Low Sodium"),
                new RadioButtonItem("DiabeticFriendly", "Diabetic Friendly"),
                new RadioButtonItem("HeartHealthy", "Heart Healthy"),
            };

            return View("~/Areas/Sandbox/Views/Components/Native/NativeCheckList/Index.cshtml", new NativeCheckListModel { Allergies = new[] { "Peanuts", "Dairy" } });
        }

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] NativeCheckListModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new { ResidentName = new[] { "Request body is required." } } });

            var validator = new NativeCheckListFormValidator();
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

            var items = model.Allergies ?? Array.Empty<string>();
            return Ok(new { message = $"Saved {items.Length} allergies and dietary needs for {model.ResidentName}" });
        }
    }
}
