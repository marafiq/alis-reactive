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

    /// <summary>The allergies the page sends to <c>POST MultiSelect/Echo</c>, which echoes its body back.</summary>
    public sealed class MultiSelectEchoRequest
    {
        public string[]? Allergies { get; set; }
    }

    /// <summary>The query of <c>GET MultiSelect/Supplies</c>: the text typed in the supplies box.</summary>
    public sealed class SupplySearchQuery
    {
        public string? Supplies { get; set; }
    }

    public class SupplySearchResponse
    {
        public List<SupplyItem> Supplies { get; set; } = new();
        public int Count { get; set; }
    }

    /// <summary>The document events the MultiSelect page raises and hears.</summary>
    public static class MultiSelectEvents
    {
        public static readonly AppEvent DoGather = AppEvent.Named("do-gather");
    }
}
