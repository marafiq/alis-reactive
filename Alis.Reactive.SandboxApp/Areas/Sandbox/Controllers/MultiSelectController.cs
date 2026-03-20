using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class MultiSelectController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Allergies = new List<AllergyItem>
            {
                new() { Value = "peanuts", Text = "Peanuts", Category = "Food" },
                new() { Value = "shellfish", Text = "Shellfish", Category = "Food" },
                new() { Value = "dairy", Text = "Dairy", Category = "Food" },
                new() { Value = "gluten", Text = "Gluten", Category = "Food" },
                new() { Value = "penicillin", Text = "Penicillin", Category = "Medication" },
                new() { Value = "aspirin", Text = "Aspirin", Category = "Medication" },
                new() { Value = "latex", Text = "Latex", Category = "Environmental" },
                new() { Value = "pollen", Text = "Pollen", Category = "Environmental" }
            };
            ViewBag.DietaryRestrictions = new List<DietaryItem>
            {
                new() { Value = "vegetarian", Text = "Vegetarian", Category = "Diet" },
                new() { Value = "vegan", Text = "Vegan", Category = "Diet" },
                new() { Value = "halal", Text = "Halal", Category = "Diet" },
                new() { Value = "kosher", Text = "Kosher", Category = "Diet" },
                new() { Value = "low-sodium", Text = "Low Sodium", Category = "Medical" },
                new() { Value = "diabetic", Text = "Diabetic", Category = "Medical" },
                new() { Value = "pureed", Text = "Pureed", Category = "Texture" },
                new() { Value = "thickened-liquids", Text = "Thickened Liquids", Category = "Texture" }
            };
            return View(new MultiSelectModel());
        }

        [HttpGet]
        public IActionResult Supplies([FromQuery] string? Supplies)
        {
            var all = new List<SupplyItem>
            {
                new() { Value = "gloves", Text = "Gloves", Category = "PPE" },
                new() { Value = "masks", Text = "Masks", Category = "PPE" },
                new() { Value = "gowns", Text = "Gowns", Category = "PPE" },
                new() { Value = "gauze", Text = "Gauze", Category = "Wound Care" },
                new() { Value = "bandages", Text = "Bandages", Category = "Wound Care" },
                new() { Value = "tape", Text = "Medical Tape", Category = "Wound Care" },
                new() { Value = "syringes", Text = "Syringes", Category = "Injection" },
                new() { Value = "needles", Text = "Needles", Category = "Injection" },
                new() { Value = "thermometer", Text = "Thermometer", Category = "Monitoring" },
                new() { Value = "stethoscope", Text = "Stethoscope", Category = "Monitoring" }
            };

            var search = Supplies == "null" ? null : Supplies;
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(s => s.Text.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || s.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(new SupplySearchResponse
            {
                Supplies = filtered,
                Count = filtered.Count
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
