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
        /// <summary>Selected values.</summary>
        public string[]? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
