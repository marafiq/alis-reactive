namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDatePicker for selecting a single date.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.AdmissionDate)</c>
    /// to access FusionDatePicker-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionDatePicker : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionDatePicker(), "datepicker");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
