using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML dropdown (<c>&lt;select&gt;</c>).
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeDropDown()</c> factory to create a model-bound dropdown with
    /// label, validation, and reactive event support.
    /// </remarks>
    public sealed class NativeDropDown : NativeComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly CapabilityMethod Focus = Method("focus");
        internal static readonly ComponentMetadata Definition = DescribeBindable("dropdown", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
