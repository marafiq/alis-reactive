namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionInPlaceEditor for click-to-edit single-field commit flows and mixed-form inputs.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.DateOfBirth)</c>
    /// to access FusionInPlaceEditor-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionInPlaceEditor : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionInPlaceEditor(), "inplace-editor");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
