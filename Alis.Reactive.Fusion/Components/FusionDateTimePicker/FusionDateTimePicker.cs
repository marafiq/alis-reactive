namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDateTimePicker for selecting a date and time together.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.AppointmentTime)</c>
    /// to access FusionDateTimePicker-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionDateTimePicker : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionDateTimePicker(), "datetimepicker");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
