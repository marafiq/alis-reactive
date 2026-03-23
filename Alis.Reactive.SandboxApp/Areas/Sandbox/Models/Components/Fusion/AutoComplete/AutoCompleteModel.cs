namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete
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

    public class MedicationSearchResponse
    {
        public List<MedicationTypeItem> Medications { get; set; } = new();
        public int Count { get; set; }
    }
}
