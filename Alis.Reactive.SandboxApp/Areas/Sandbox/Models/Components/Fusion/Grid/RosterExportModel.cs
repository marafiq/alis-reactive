namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RosterExportModel
    {
        public string Month { get; set; } = "";
    }

    public class MonthlyCensusQuery
    {
        public string? Month { get; set; }
    }

    public class MonthlyCensus
    {
        public List<MonthlyCensusResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class MonthlyCensusResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";

        public int CensusDays { get; set; }
    }
}
