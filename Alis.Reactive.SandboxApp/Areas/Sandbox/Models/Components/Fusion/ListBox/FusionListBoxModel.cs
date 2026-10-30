namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListBox
{
    /// <summary>
    /// The activities coordinator splitting the residents signed up for Thursday's garden outing between the
    /// morning bus and the afternoon bus.
    /// </summary>
    public sealed class FusionListBoxModel
    {
        /// <summary>Whether residents with a morning appointment are kept for the afternoon bus.</summary>
        public bool MorningAppointmentsRideLater { get; set; }
    }

    /// <summary>A resident signed up for the outing.</summary>
    /// <param name="Id">The resident's ID, the list's value.</param>
    /// <param name="Name">The resident's name, as the list shows it.</param>
    public sealed record OutingResident(string Id, string Name);

    /// <summary>Thursday, October 16: the trip to Tower Hill Botanic Garden.</summary>
    public static class OutingDay
    {
        /// <summary>The seats on the morning bus.</summary>
        public const int MorningSeats = 6;

        /// <summary>Everyone signed up, in the order they signed up.</summary>
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

        /// <summary>The residents in wheelchairs; the wheelchair lift is on the morning bus only.</summary>
        public static readonly IReadOnlyList<string> LiftRiders = new[] { "walter-dean", "margaret-hale" };

        /// <summary>The residents with an appointment while the morning bus is out.</summary>
        public static readonly IReadOnlyList<string> MorningAppointments = new[] { "ruth-coleman", "dorothy-kim" };

        /// <summary>Who rode the morning bus last Thursday.</summary>
        public static readonly IReadOnlyList<string> LastWeekMorning = new[] { "arthur-bell", "grace-liu", "samuel-ortiz" };
    }

    /// <summary>The morning bus's riders, to save.</summary>
    public sealed class OutingGroupsRequest
    {
        /// <summary>The IDs of the residents on the morning bus.</summary>
        public string[] Morning { get; set; } = [];
    }

    /// <summary>What the office has on file for the outing.</summary>
    public sealed class OutingGroupsSaved
    {
        /// <summary>Who rides in the morning, and how many ride in the afternoon.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
