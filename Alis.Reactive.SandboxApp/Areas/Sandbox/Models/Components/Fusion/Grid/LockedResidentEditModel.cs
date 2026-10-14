namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The resident records page: the families' requests waiting for the records coordinator.</summary>
    public class ResidentRecordsViewModel
    {
        public List<RecordUpdateRequest> Requests { get; set; } = new();
    }

    /// <summary>A family's request to update a resident's record, and the button that opens the record.</summary>
    public sealed record RecordUpdateRequest(string ButtonId, int ResidentId, string Request, string ButtonText);

    /// <summary>A resident's record as the front office keeps it, one row of the records grid.</summary>
    public class ResidentContactRecord
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";

        /// <summary><c>Active</c>, or <c>Discharged</c> once the resident has left the community.</summary>
        public string Status { get; set; } = "";

        /// <summary>The day a discharged resident left, as the page says it: <c>September 30</c>; empty while the resident lives here.</summary>
        public string DischargedOn { get; set; } = "";

        /// <summary>The number the front office calls the family on.</summary>
        public string FamilyPhone { get; set; } = "";
    }
}
