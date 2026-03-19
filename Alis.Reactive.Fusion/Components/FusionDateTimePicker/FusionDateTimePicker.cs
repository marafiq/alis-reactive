namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion DateTimePicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionDateTimePicker&gt;(m => m.AppointmentTime) to unlock
    /// the DateTimePicker-specific extension methods.
    /// </summary>
    public sealed class FusionDateTimePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
