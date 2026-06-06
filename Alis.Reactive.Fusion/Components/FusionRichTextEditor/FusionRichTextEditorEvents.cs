namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionRichTextEditor"/> component.
    /// </summary>
    public sealed class FusionRichTextEditorEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionRichTextEditorEvents Instance = new FusionRichTextEditorEvents();
        private FusionRichTextEditorEvents() { }

        /// <summary>Fires when the rich text content changes.</summary>
        public TypedEvent<FusionRichTextEditorChangeArgs> Changed =>
            new TypedEvent<FusionRichTextEditorChangeArgs>(
                "change", new FusionRichTextEditorChangeArgs());
    }
}
