using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDateTimePicker for selecting a date and time together.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.AppointmentTime)</c>
    /// to access FusionDateTimePicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDateTimePicker : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("datetimepicker", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
