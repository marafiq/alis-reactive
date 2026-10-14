namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The monthly census page: the month the business office reports, as <c>yyyy-MM</c>.</summary>
    public class RosterExportModel
    {
        public string Month { get; set; } = "";
    }

    /// <summary>The month whose census the page asks for, as <c>yyyy-MM</c>.</summary>
    public class MonthlyCensusQuery
    {
        public string? Month { get; set; }
    }

    /// <summary>A month's census: who lived here and for how many days, or why the month cannot be reported.</summary>
    public class MonthlyCensus
    {
        public List<MonthlyCensusResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident on a month's census, one row of the census grid.</summary>
    public class MonthlyCensusResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";

        /// <summary>The days of the month the resident lived here, which the month's billing counts.</summary>
        public int CensusDays { get; set; }
    }
}
