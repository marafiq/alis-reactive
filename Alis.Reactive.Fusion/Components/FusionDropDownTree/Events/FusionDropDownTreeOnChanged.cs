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
        /// <summary>Selected value IDs after the change event.</summary>
        public string[]? Value { get; set; }

        /// <summary>Previous selected value IDs.</summary>
        public string[]? OldValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
