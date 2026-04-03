namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionRichTextEditor"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionRichTextEditorEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionRichTextEditorEvents Instance = new FusionRichTextEditorEvents();
        private FusionRichTextEditorEvents() { }

        /// <summary>Fires when the rich text content changes (SF "change" event).</summary>
        public ReactiveEvent<FusionRichTextEditorChangeArgs> Changed =>
            new ReactiveEvent<FusionRichTextEditorChangeArgs>(
                "change", new FusionRichTextEditorChangeArgs());
    }
}
