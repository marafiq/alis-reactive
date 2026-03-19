namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion RichTextEditor component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionRichTextEditor&gt;(m => m.CarePlan) to unlock
    /// the RichTextEditor-specific extension methods.
    /// </summary>
    public sealed class FusionRichTextEditor : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
