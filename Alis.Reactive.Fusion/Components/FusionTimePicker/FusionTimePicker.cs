namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion TimePicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionTimePicker&gt;(m => m.MedicationTime) to unlock
    /// the TimePicker-specific extension methods.
    /// </summary>
    public sealed class FusionTimePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
