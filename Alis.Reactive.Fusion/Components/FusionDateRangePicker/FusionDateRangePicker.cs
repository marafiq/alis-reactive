namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion date-range picker component for selecting start and end dates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use as a type parameter in <c>p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayPeriod)</c>
    /// to access FusionDateRangePicker-specific component operations and value reads.
    /// </para>
    /// <para>
    /// Full value is a <c>DateTime[]</c> containing both dates. For targeted access to
    /// individual dates, use <c>comp.StartDate()</c> or <c>comp.EndDate()</c> in conditions.
    /// </para>
    /// </remarks>
    public sealed class FusionDateRangePicker : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionDateRangePicker(), "daterangepicker");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
