namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The resident roster page: the roster comes from the resident record, so the page has no fields of its own.</summary>
    public class ResidentRosterViewModel
    {
    }

    /// <summary>A resident as the resident record lists them.</summary>
    public class RecordRosterResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    /// <summary>A wing's residents as the record answers them, and the line the page shows about them.</summary>
    public class RecordRosterPage
    {
        public List<RecordRosterResident> Result { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>The wing the roster shows.</summary>
    public class RecordRosterWingQuery
    {
        public string Wing { get; set; } = "";
    }

    /// <summary>The record's answer for one wing: the wing, and its page of residents.</summary>
    public class RecordRosterWing
    {
        public string Wing { get; set; } = "";
        public RecordRosterPage Page { get; set; } = new();
    }

    /// <summary>Today's arrival, and the place her suite takes on the roster.</summary>
    public class RecordRosterArrival
    {
        public RecordRosterResident Resident { get; set; } = new();
        public int Position { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident's row as the record now has it, and what changed.</summary>
    public class RecordRosterUpdate
    {
        public RecordRosterResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
