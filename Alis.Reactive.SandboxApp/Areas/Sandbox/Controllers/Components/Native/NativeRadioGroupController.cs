using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Move-in preferences: the move-in coordinator records Dorothy Price's care level, room type and meal
    /// plan before her move-in day, and the office records them.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeRadioGroup")]
    public class NativeRadioGroupController : Controller
    {
        private const int DorothyPriceId = 4127;

        private static readonly IReadOnlyList<RadioButtonItem> RoomTypes = new[]
        {
            new RadioButtonItem("Shared room", "Shared room", "A bedroom shared with one resident"),
            new RadioButtonItem("Private room", "Private room", "A bedroom of her own with a shared bath"),
            new RadioButtonItem("Private suite", "Private suite", "Bedroom, sitting room and kitchenette"),
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.RoomTypes = RoomTypes;
            return View("~/Areas/Sandbox/Views/Components/Native/NativeRadioGroup/Index.cshtml", new NativeRadioGroupModel { ResidentId = DorothyPriceId });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] NativeRadioGroupModel? preferences)
        {
            if (preferences == null || preferences.ResidentId != DorothyPriceId) return BadRequest();

            var result = new MoveInPreferencesValidator().Validate(preferences);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new MoveInPreferencesSaved
            {
                Message = $"Move-in preferences saved for Dorothy Price: {preferences.CareLevel}, {preferences.RoomType}, {preferences.MealPlan} meals.",
            });
        }
    }
}
