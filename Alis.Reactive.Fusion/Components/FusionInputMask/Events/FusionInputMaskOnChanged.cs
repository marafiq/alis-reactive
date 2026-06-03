namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionInputMask"/> value changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.
    /// </remarks>
    public class FusionInputMaskChangeArgs
    {
        /// <summary>Gets or sets the new masked value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionInputMaskChangeArgs() { }
    }
}
