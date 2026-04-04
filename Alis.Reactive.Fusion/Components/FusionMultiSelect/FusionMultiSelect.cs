using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionMultiSelect for choosing multiple values from a list.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Allergies)</c>
    /// to access FusionMultiSelect-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("multiselect", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
