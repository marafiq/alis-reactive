namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RosterSelectionModel
    {
        public int BusSeats { get; set; }
        public int TurnedAwayResidentId { get; set; }
        public string TurnedAwayResidentName { get; set; } = "";
    }

    public class GardenOutingSignUp
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Mobility { get; set; } = "";
        public string SignedUp { get; set; } = "";
    }

    public class GardenOutingBooking
    {
        public List<GardenOutingSignUp> Riders { get; set; } = new();
    }

    public class GardenOutingBooked
    {
        public string Summary { get; set; } = "";
    }
}
