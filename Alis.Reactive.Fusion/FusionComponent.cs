using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Base type for all Syncfusion-backed components.
    /// </summary>
    /// <remarks>
    /// Sealed subclasses (e.g. <see cref="Components.FusionDropDownList"/>,
    /// <see cref="Components.FusionDatePicker"/>) serve as type parameters in
    /// <c>p.Component&lt;T&gt;()</c> to scope which extension methods are available.
    /// </remarks>
    public abstract class FusionComponent : IComponent, IReactiveComponentDescriptor
    {
        /// <inheritdoc />
        public string Vendor => "fusion";

        internal abstract ComponentMetadata Metadata { get; }
        ComponentMetadata IReactiveComponentDescriptor.Metadata => Metadata;

        internal static ComponentMetadata Describe(string kind) => ComponentMetadata.Create("fusion", kind, binding: null);
        internal static ComponentMetadata DescribeBindable(
            string kind,
            CapabilityProperty binding,
            ValueShape? bindingShape = null) =>
            ComponentMetadata.Create("fusion", kind, binding, bindingShape);

        internal static CapabilityProperty Property(string name) => CapabilityProperty.Named(name);
        internal static CapabilityProperty Property(string name, params PathSegment[] path) => CapabilityProperty.FromSegments(name, path);
        internal static CapabilityMethod Method(string name) => CapabilityMethod.Named(name);
        internal static CapabilityMethod Method(string name, params PathSegment[] path) => CapabilityMethod.FromSegments(name, path);
    }
}
