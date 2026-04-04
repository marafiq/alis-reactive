using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionSwitch for toggling a boolean on/off.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionSwitch&gt;(m =&gt; m.ReceiveNotifications)</c>
    /// to access FusionSwitch-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionSwitch : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Checked = Property("checked");
        internal static readonly ComponentMetadata Definition = DescribeBindable("switch", Checked);
        internal override ComponentMetadata Metadata => Definition;
    }
}
