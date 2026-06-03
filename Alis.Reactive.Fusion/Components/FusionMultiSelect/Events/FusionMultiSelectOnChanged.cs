namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionMultiSelect"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.
    /// </remarks>
    public class FusionMultiSelectChangeArgs
    {
        /// <summary>Gets or sets the selected values as a string array.</summary>
        public string[]? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionMultiSelectChangeArgs() { }
    }
}
