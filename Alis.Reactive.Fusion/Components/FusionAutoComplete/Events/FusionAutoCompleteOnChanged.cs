namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionAutoComplete"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).Eq("Dr. Smith")</c>.
    /// </remarks>
    public class FusionAutoCompleteChangeArgs
    {
        /// <summary>Gets or sets the selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionAutoCompleteChangeArgs() { }
    }
}
