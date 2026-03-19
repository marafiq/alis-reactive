namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionRichTextEditor.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).NotNull()
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// CoercionTypes infers string => "string" for runtime coercion.
    /// </summary>
    public class FusionRichTextEditorChangeArgs
    {
        /// <summary>New HTML content value.</summary>
        public string? Value { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionRichTextEditorChangeArgs() { }
    }
}
