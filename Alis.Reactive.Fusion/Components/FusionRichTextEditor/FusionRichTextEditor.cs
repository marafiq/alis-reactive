namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionRichTextEditor for editing HTML content.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.CarePlan)</c>
    /// to access FusionRichTextEditor-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionRichTextEditor : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionRichTextEditor(), "richtexteditor");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
