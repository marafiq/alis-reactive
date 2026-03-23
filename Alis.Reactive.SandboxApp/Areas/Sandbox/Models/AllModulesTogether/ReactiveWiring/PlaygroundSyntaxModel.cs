namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.AllModulesTogether.ReactiveWiring
{
    /// <summary>
    /// View model for the PlaygroundSyntax page.
    /// Exercises both Fusion (SF) and Native (DOM) components
    /// through the unified p.Component&lt;T&gt;() pipeline.
    /// </summary>
    public class PlaygroundSyntaxModel
    {
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }

        /// <summary>
        /// Nested property — exercises underscore ID pattern (Address_City)
        /// and dot-notation binding path (Address.City).
        /// </summary>
        public PlaygroundAddress? Address { get; set; }
    }

    public class PlaygroundAddress
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public int PostalCode { get; set; }
    }
}
