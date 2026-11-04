namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentGridItem
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    public class ResidentGridResponse
    {
        public List<ResidentGridItem> Result { get; set; } = new();
        public int Count { get; set; }
    }

    public class ResidentDirectoryGridItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public int OpenTasks { get; set; }
        public string NextReviewDate { get; set; } = "";
    }

    public class ResidentGridEditingModel
    {
        public string? ResidentName { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? OpenTasks { get; set; }
    }

    public class GridDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
