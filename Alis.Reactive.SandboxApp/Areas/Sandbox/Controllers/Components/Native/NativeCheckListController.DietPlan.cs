using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Diet plan: the kitchen's dietary restrictions for one resident, opened with the restrictions
    /// on file and saved back as the nurse leaves them.
    /// </summary>
    public partial class NativeCheckListController
    {
        private const int HaroldJenningsId = 3051;

        private static readonly RadioButtonItem[] DietaryRestrictionOptions =
        {
            new RadioButtonItem("Gluten-free", "Gluten-free"),
            new RadioButtonItem("Low sodium", "Low sodium"),
            new RadioButtonItem("Pureed texture", "Pureed texture"),
            new RadioButtonItem("Diabetic", "Diabetic"),
        };

        [HttpGet("DietPlan")]
        public IActionResult DietPlan()
        {
            ViewBag.DietaryRestrictionOptions = DietaryRestrictionOptions;
            var dietPlanOnFile = new DietPlanModel
            {
                ResidentId = HaroldJenningsId,
                DietaryRestrictions = new List<string> { "Gluten-free", "Low sodium" },
            };
            return View("~/Areas/Sandbox/Views/Components/Native/NativeCheckList/DietPlan.cshtml", dietPlanOnFile);
        }

        [HttpPost("DietPlan/Save")]
        public IActionResult SaveDietPlan([FromBody] DietPlanModel? dietPlan)
        {
            if (dietPlan == null || dietPlan.ResidentId != HaroldJenningsId) return BadRequest();

            var restrictions = dietPlan.DietaryRestrictions.Count == 0
                ? "no dietary restrictions"
                : string.Join(", ", dietPlan.DietaryRestrictions);
            return Ok(new DietPlanSaved($"Diet plan saved for Harold Jennings: {restrictions}."));
        }
    }
}
