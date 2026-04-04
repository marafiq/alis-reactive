using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionAutoComplete for typing and filtering suggestions from a data source.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician)</c>
    /// to access FusionAutoComplete-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionAutoComplete : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("autocomplete", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
