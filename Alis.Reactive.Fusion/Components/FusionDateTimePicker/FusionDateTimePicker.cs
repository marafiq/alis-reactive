namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion DateTimePicker for selecting a date and time together.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.AppointmentTime)</c>
    /// to access DateTimePicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDateTimePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
