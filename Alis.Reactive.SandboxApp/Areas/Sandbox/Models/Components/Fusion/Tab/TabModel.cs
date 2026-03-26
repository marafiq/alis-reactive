namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the FusionTab sandbox demo page.
    /// Tab is a non-input component — this model carries no tab-bound properties.
    /// Properties here support echo elements and button interactions.
    /// </summary>
    public class TabModel
    {
        /// <summary>Placeholder — model is required by Razor but Tab has no form value.</summary>
        public string? Placeholder { get; set; }
    }
}
