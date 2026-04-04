using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionMultiColumnComboBox for selecting a value with a multi-column dropdown.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility)</c>
    /// to access FusionMultiColumnComboBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionMultiColumnComboBox : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("multicolumncombobox", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
