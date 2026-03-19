namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionRichTextEditor.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionRichTextEditorEvents
    {
        public static readonly FusionRichTextEditorEvents Instance = new FusionRichTextEditorEvents();
        private FusionRichTextEditorEvents() { }

        /// <summary>Fires when the rich text content changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionRichTextEditorChangeArgs> Changed =>
            new TypedEventDescriptor<FusionRichTextEditorChangeArgs>(
                "change", new FusionRichTextEditorChangeArgs());
    }
}
