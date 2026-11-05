namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeRadioGroupModel
    {
        public int ResidentId { get; set; }

        public string? CareLevel { get; set; }

        public string? RoomType { get; set; }

        public string? MealPlan { get; set; }
    }

    public sealed class MoveInReferral
    {
        public string CareLevel { get; set; } = "";
    }

    public sealed class MoveInPreferencesSaved
    {
        public string Message { get; set; } = "";
    }
}
