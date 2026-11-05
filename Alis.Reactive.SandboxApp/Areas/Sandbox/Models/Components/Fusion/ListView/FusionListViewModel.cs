namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListView
{
    public sealed class FusionListViewModel
    {
        public string? HandoffResident { get; set; }
    }

    public sealed record RoundResident(string Name, string Note, string Away = "");

    public static class MapleHandoff
    {
        public static readonly IReadOnlyList<RoundResident> Round = new[]
        {
            new RoundResident("Arthur Bell", "Blood sugar 182 at lunch. Recheck before dinner and call Dr. Shah if it is over 200."),
            new RoundResident("Ruth Coleman", "New walker delivered today. Physical therapy at 4 PM; encourage fluids."),
            new RoundResident("Helen Park", "", Away: "St. Mary's Hospital until Thursday"),
            new RoundResident("Walter Dean", "Refused breakfast and lunch. His daughter visits at 6 PM; let her know."),
            new RoundResident("Grace Liu", "Fell last night without injury. Bed alarm on; check on her every two hours.")
        };

        public static RoundResident Away => Round.Single(resident => resident.Away.Length > 0);

        public static readonly IReadOnlyList<string> BeforeLeaving = new[]
        {
            "Narcotics count matched",
            "New orders read back",
            "Call lights tested",
            "Incidents logged",
            "Med cart keys handed over"
        };
    }

    public sealed class RoundNoteQuery
    {
        public int? Place { get; set; }
    }

    public sealed class RoundNote
    {
        public string Resident { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }

    public sealed class HandOffRequest
    {
        public string Resident { get; set; } = string.Empty;
    }

    public sealed class HandOffAnswer
    {
        public string HandedOff { get; set; } = string.Empty;

        public string Next { get; set; } = string.Empty;

        public string NextNote { get; set; } = string.Empty;
    }
}
