using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Test widget for architecture verification — fusion vendor.
    /// Phantom type — proves vendor resolves root via ej2_instances[0],
    /// and bindingMember walks from that root.
    /// </summary>
    public sealed class TestWidgetSyncFusion : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly ComponentMetadata Definition =
            DescribeBindable("testwidget-syncfusion", Value, ValueShapeFactory.String());
        internal override ComponentMetadata Metadata => Definition;
    }
}
