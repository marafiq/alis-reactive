using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionRichTextEditor for editing HTML content.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.CarePlan)</c>
    /// to access FusionRichTextEditor-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionRichTextEditor : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("richtexteditor", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
