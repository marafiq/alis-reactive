namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListBox
{
    public sealed class FusionListBoxModel
    {
        public bool MorningAppointmentsRideLater { get; set; }
    }

    public sealed record OutingResident(string Id, string Name);

    public static class OutingDay
    {
        public const int MorningSeats = 6;

        public static readonly IReadOnlyList<OutingResident> SignedUp = new[]
        {
            new OutingResident("arthur-bell", "Arthur Bell"),
            new OutingResident("ruth-coleman", "Ruth Coleman"),
            new OutingResident("walter-dean", "Walter Dean"),
            new OutingResident("grace-liu", "Grace Liu"),
            new OutingResident("margaret-hale", "Margaret Hale"),
            new OutingResident("samuel-ortiz", "Samuel Ortiz"),
            new OutingResident("dorothy-kim", "Dorothy Kim"),
            new OutingResident("frank-russo", "Frank Russo")
        };

        public static readonly IReadOnlyList<string> LiftRiders = new[] { "walter-dean", "margaret-hale" };

        public static readonly IReadOnlyList<string> MorningAppointments = new[] { "ruth-coleman", "dorothy-kim" };

        public static readonly IReadOnlyList<string> LastWeekMorning = new[] { "arthur-bell", "grace-liu", "samuel-ortiz" };
    }

    public sealed class OutingGroupsRequest
    {
        public string[] Morning { get; set; } = [];
    }

    public sealed class OutingGroupsSaved
    {
        public string Message { get; set; } = string.Empty;
    }
}
