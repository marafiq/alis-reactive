namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDropDownTree"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).NotEmpty()</c>.
    /// </remarks>
    public class FusionDropDownTreeChangeArgs
    {
        /// <summary>Gets or sets the selected value IDs.</summary>
        public string[]? Value { get; set; }

        /// <summary>Gets or sets the previous selected value IDs.</summary>
        public string[]? OldValue { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionDropDownTreeChangeArgs() { }
    }
}
