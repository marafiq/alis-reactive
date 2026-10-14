namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The garden outing sign-up page: how many seats the bus has, and the resident the bus left behind last
    /// time. The grid holds the sign-ups, so the page has no fields of its own.
    /// </summary>
    public class RosterSelectionModel
    {
        public int BusSeats { get; set; }
        public int TurnedAwayResidentId { get; set; }
        public string TurnedAwayResidentName { get; set; } = "";
    }

    /// <summary>A resident who asked to go on the outing, one row of the sign-up sheet.</summary>
    public class GardenOutingSignUp
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Mobility { get; set; } = "";
        public string SignedUp { get; set; } = "";
    }

    /// <summary>The residents seated on the bus, as the director books it.</summary>
    public class GardenOutingBooking
    {
        public List<GardenOutingSignUp> Riders { get; set; } = new();
    }

    /// <summary>What a booking did, or why the bus was not booked.</summary>
    public class GardenOutingBooked
    {
        public string Summary { get; set; } = "";
    }
}
