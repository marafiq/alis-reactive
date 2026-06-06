namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion in-place editor for click-to-edit single-field commit flows and mixed-form inputs.
    /// </summary>
    /// <remarks>
    /// Use as a component type in <c>p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.DateOfBirth)</c>
    /// to control edit mode, commit values, or read the registered value shape.
    /// </remarks>
    public sealed class FusionInPlaceEditor : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionInPlaceEditor(), "inplace-editor");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
