namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionRichTextEditor"/> component.
    /// </summary>
    public sealed class FusionRichTextEditorEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionRichTextEditorEvents Instance = new FusionRichTextEditorEvents();
        private FusionRichTextEditorEvents() { }

        /// <summary>Fires when the rich text content changes.</summary>
        public TypedEvent<FusionRichTextEditorChangeArgs> Changed =>
            new TypedEvent<FusionRichTextEditorChangeArgs>(
                "change", new FusionRichTextEditorChangeArgs());
    }
}
