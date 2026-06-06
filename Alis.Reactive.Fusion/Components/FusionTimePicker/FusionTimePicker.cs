namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion time picker component for selecting a time value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionTimePicker&gt;(m =&gt; m.MedicationTime)</c>
    /// to access FusionTimePicker-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionTimePicker : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionTimePicker(), "timepicker");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
