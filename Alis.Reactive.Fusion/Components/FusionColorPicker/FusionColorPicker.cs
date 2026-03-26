namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion ColorPicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionColorPicker&gt;(m => m.ThemeColor) to unlock
    /// the ColorPicker-specific extension methods.
    /// </summary>
    public sealed class FusionColorPicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
