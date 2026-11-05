namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentRecordsViewModel
    {
        public List<RecordUpdateRequest> Requests { get; set; } = new();
    }

    public sealed record RecordUpdateRequest(string ButtonId, int ResidentId, string Request, string ButtonText);

    public class ResidentContactRecord
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";

        public string Status { get; set; } = "";

        public string DischargedOn { get; set; } = "";

        public string FamilyPhone { get; set; } = "";
    }
}
