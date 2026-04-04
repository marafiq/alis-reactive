using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;button&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Not an IBindableComponent — buttons have no form value to read.
    /// </summary>
    public sealed class NativeButton : NativeComponent
    {
        internal static readonly CapabilityProperty Text = Property("text", PathSegment.FromProp("textContent"));
        internal static readonly CapabilityMethod Focus = Method("focus");
        internal static readonly ComponentMetadata Definition = Describe("button");
        internal override ComponentMetadata Metadata => Definition;
    }
}
