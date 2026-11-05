namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentRosterViewModel
    {
    }

    public class RecordRosterResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    public class RecordRosterPage
    {
        public List<RecordRosterResident> Result { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class RecordRosterWingQuery
    {
        public string Wing { get; set; } = "";
    }

    public class RecordRosterWing
    {
        public string Wing { get; set; } = "";
        public RecordRosterPage Page { get; set; } = new();
    }

    public class RecordRosterArrival
    {
        public RecordRosterResident Resident { get; set; } = new();
        public int Position { get; set; }
        public string Summary { get; set; } = "";
    }

    public class RecordRosterUpdate
    {
        public RecordRosterResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
