using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeCheckList")]
    public partial class NativeCheckListController : Controller
    {
        [HttpGet("")]
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
                return BadRequest(new { errors = result.ErrorsByField });

            var items = model.Allergies ?? Array.Empty<string>();
            return Ok(new { message = $"Saved {items.Length} allergies and dietary needs for {model.ResidentName}" });
        }
    }
}
