namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion DatePicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionDatePicker&gt;(m => m.AdmissionDate) to unlock
    /// the DatePicker-specific extension methods.
    /// </summary>
    public sealed class FusionDatePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
