using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionColorPicker for selecting a color value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionColorPicker&gt;(m =&gt; m.ThemeColor)</c>
    /// to access FusionColorPicker-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionColorPicker : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition = DescribeBindable("colorpicker", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
