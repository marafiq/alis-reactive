using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native
{
    /// <summary>
    /// Base class for all native HTML components (text inputs, checkboxes, dropdowns, etc.).
    /// </summary>
    /// <remarks>
    /// Sealed subclasses constrain which extension methods are available at compile time.
    /// For example, <c>SetChecked</c> only appears on <see cref="ComponentRef{TComponent,TModel}"/>
    /// when <c>TComponent</c> is <c>NativeCheckBox</c>.
    /// </remarks>
    public abstract class NativeComponent : IComponent, IReactiveComponentDescriptor
    {
        /// <inheritdoc />
        public string Vendor => "native";

        internal abstract ComponentMetadata Metadata { get; }
        ComponentMetadata IReactiveComponentDescriptor.Metadata => Metadata;

        internal static ComponentMetadata Describe(string kind) => ComponentMetadata.Create("native", kind, binding: null);
        internal static ComponentMetadata DescribeBindable(
            string kind,
            CapabilityProperty binding,
            ValueShape? bindingShape = null) =>
            ComponentMetadata.Create("native", kind, binding, bindingShape);

        internal static CapabilityProperty Property(string name) => CapabilityProperty.Named(name);
        internal static CapabilityProperty Property(string name, params PathSegment[] path) => CapabilityProperty.FromSegments(name, path);
        internal static CapabilityMethod Method(string name) => CapabilityMethod.Named(name);
        internal static CapabilityMethod Method(string name, params PathSegment[] path) => CapabilityMethod.FromSegments(name, path);
    }
}
