using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Test widget for architecture verification — native vendor.
    /// Phantom type — proves the same bindingMember works for both vendors.
    /// </summary>
    public sealed class TestWidgetNative : NativeComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly CapabilityMethod Focus = Method("focus");
        internal static readonly ComponentMetadata Definition =
            DescribeBindable("testwidget-native", Value, ValueShapeFactory.String());
        internal override ComponentMetadata Metadata => Definition;
    }
}
