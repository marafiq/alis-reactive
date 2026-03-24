namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MultiSelectModel
    {
        public string[]? Allergies { get; set; }
        public string[]? DietaryRestrictions { get; set; }
        public string[]? Supplies { get; set; }
    }

    public class AllergyItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class DietaryItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class SupplyItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class SupplySearchResponse
    {
        public List<SupplyItem> Supplies { get; set; } = new();
        public int Count { get; set; }
    }
}
