namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDropDownList"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).Eq("US")</c>.
    /// </remarks>
    public class FusionDropDownListChangeArgs
    {
        /// <summary>Gets or sets the selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }
        public FusionDropDownListChangeArgs() { }
    }
}
