using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDateRangePicker for selecting a start and end date pair.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use as a type parameter in <c>p.Component&lt;FusionDateRangePicker&gt;(m =&gt; m.StayPeriod)</c>
    /// to access FusionDateRangePicker-specific mutations and value reading.
    /// </para>
    /// <para>
    /// The full value is a <c>DateTime[]</c> containing both dates. For targeted access to
    /// individual dates, use <c>comp.StartDate()</c> or <c>comp.EndDate()</c> in conditions.
    /// </para>
    /// </remarks>
    public sealed class FusionDateRangePicker : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("daterangepicker", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
