namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion ComboBox component (AutoComplete with filtering/autocomplete).
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionComboBox&gt;(m => m.Physician) to unlock
    /// the ComboBox-specific extension methods.
    /// </summary>
    public sealed class FusionComboBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
