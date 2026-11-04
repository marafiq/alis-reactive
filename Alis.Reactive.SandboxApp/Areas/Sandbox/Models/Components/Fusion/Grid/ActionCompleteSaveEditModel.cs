namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class PharmacyRosterResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
    }

    public class PharmacyNameCorrection
    {
        public int ResidentId { get; set; }
        public string PreviousName { get; set; } = "";
        public string CorrectedName { get; set; } = "";

        public int? RosterLine { get; set; }
    }

    public class PharmacyNotice
    {
        public string Summary { get; set; } = "";
    }
}
