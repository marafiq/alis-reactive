namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Playground syntax state shared by Fusion and Native components
    /// through the unified <c>p.Component&lt;T&gt;()</c> pipeline.
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
