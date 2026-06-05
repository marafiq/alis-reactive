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
        /// <summary>Selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
