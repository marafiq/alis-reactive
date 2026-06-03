namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionRichTextEditor"/> content changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.
    /// </remarks>
    public class FusionRichTextEditorChangeArgs
    {
        /// <summary>Gets or sets the new HTML content value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionRichTextEditorChangeArgs() { }
    }
}
