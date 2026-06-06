namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionColorPicker for selecting a color value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor)</c>
    /// to access FusionColorPicker-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionColorPicker : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionColorPicker(), "colorpicker");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
