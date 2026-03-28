namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion ColorPicker for selecting a color value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor)</c>
    /// to access ColorPicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionColorPicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
