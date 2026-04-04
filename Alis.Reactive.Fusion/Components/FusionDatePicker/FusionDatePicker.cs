using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionDatePicker for selecting a single date.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.AdmissionDate)</c>
    /// to access FusionDatePicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionDatePicker : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("datepicker", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
