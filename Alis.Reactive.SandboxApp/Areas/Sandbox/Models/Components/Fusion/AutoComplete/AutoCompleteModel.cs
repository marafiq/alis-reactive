namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class AutoCompleteModel
    {
        public string? Physician { get; set; }
        public string? MedicationType { get; set; }
    }

    public class PhysicianItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Specialty { get; set; } = "";
    }

    public class MedicationTypeItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Category { get; set; } = "";
    }

    /// <summary>The search text the medication AutoComplete sends to <c>GET AutoComplete/Medications</c>.</summary>
    public sealed class MedicationSearchQuery
    {
        public string MedicationType { get; set; } = "";
    }

    public class MedicationSearchResponse
    {
        public List<MedicationTypeItem> Medications { get; set; } = new();
        public int Count { get; set; }
    }

    /// <summary>The document events the AutoComplete page raises and hears.</summary>
    public static class AutoCompleteEvents
    {
        public static readonly AppEvent DoGather = AppEvent.Named("do-gather");
    }
}
