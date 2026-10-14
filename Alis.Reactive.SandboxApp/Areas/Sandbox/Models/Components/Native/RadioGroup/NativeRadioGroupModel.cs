namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A new resident's move-in preferences, recorded by the move-in coordinator before move-in day: the
    /// care level, the room type and the meal plan.
    /// </summary>
    public class NativeRadioGroupModel
    {
        public int ResidentId { get; set; }

        public string? CareLevel { get; set; }

        public string? RoomType { get; set; }

        public string? MealPlan { get; set; }
    }

    /// <summary>The hospital referral that sends a new resident to the community, with the care level it names.</summary>
    public sealed class MoveInReferral
    {
        public string CareLevel { get; set; } = "";
    }

    /// <summary>The office's answer once the move-in preferences are recorded.</summary>
    public sealed class MoveInPreferencesSaved
    {
        public string Message { get; set; } = "";
    }
}
