namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListView
{
    /// <summary>
    /// The day charge nurse on Maple wing handing the residents over to the evening charge nurse, one by one
    /// along the round, then going through what to finish before leaving.
    /// </summary>
    public sealed class FusionListViewModel
    {
        /// <summary>The resident whose handoff note is open.</summary>
        public string? HandoffResident { get; set; }
    }

    /// <summary>A resident on the round and what the evening nurse needs to know.</summary>
    /// <param name="Name">The resident, as the round lists them.</param>
    /// <param name="Note">The handoff note.</param>
    /// <param name="Away">Where the resident is when they are out of the building, or empty.</param>
    public sealed record RoundResident(string Name, string Note, string Away = "");

    /// <summary>Maple wing's round, in walking order, and what to finish before leaving.</summary>
    public static class MapleHandoff
    {
        /// <summary>The residents in the order the round reaches their rooms.</summary>
        public static readonly IReadOnlyList<RoundResident> Round = new[]
        {
            new RoundResident("Arthur Bell", "Blood sugar 182 at lunch. Recheck before dinner and call Dr. Shah if it is over 200."),
            new RoundResident("Ruth Coleman", "New walker delivered today. Physical therapy at 4 PM; encourage fluids."),
            new RoundResident("Helen Park", "", Away: "St. Mary's Hospital until Thursday"),
            new RoundResident("Walter Dean", "Refused breakfast and lunch. His daughter visits at 6 PM; let her know."),
            new RoundResident("Grace Liu", "Fell last night without injury. Bed alarm on; check on her every two hours.")
        };

        /// <summary>The resident out of the building, whose handoff went with them.</summary>
        public static RoundResident Away => Round.Single(resident => resident.Away.Length > 0);

        /// <summary>What the day nurse covers before leaving the wing.</summary>
        public static readonly IReadOnlyList<string> BeforeLeaving = new[]
        {
            "Narcotics count matched",
            "New orders read back",
            "Call lights tested",
            "Incidents logged",
            "Med cart keys handed over"
        };
    }

    /// <summary>The handoff note for the resident at a place on the round.</summary>
    public sealed class RoundNoteQuery
    {
        /// <summary>The resident's zero-based place on the round.</summary>
        public int? Place { get; set; }
    }

    /// <summary>A resident's handoff note.</summary>
    public sealed class RoundNote
    {
        /// <summary>The resident.</summary>
        public string Resident { get; set; } = string.Empty;

        /// <summary>What the evening nurse needs to know.</summary>
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>The resident the day nurse has handed off.</summary>
    public sealed class HandOffRequest
    {
        /// <summary>The resident handed off.</summary>
        public string Resident { get; set; } = string.Empty;
    }

    /// <summary>Who comes next on the round once a resident is handed off.</summary>
    public sealed class HandOffAnswer
    {
        /// <summary>The resident handed off.</summary>
        public string HandedOff { get; set; } = string.Empty;

        /// <summary>The next resident in the building on the round, or empty when the round is complete.</summary>
        public string Next { get; set; } = string.Empty;

        /// <summary>The next resident's handoff note, or empty when the round is complete.</summary>
        public string NextNote { get; set; } = string.Empty;
    }
}
