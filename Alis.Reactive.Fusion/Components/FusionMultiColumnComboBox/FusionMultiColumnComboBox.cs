namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion MultiColumnComboBox component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionMultiColumnComboBox&gt;(m => m.Facility) to unlock
    /// the MultiColumnComboBox-specific extension methods.
    /// </summary>
    public sealed class FusionMultiColumnComboBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
