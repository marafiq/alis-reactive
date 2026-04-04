using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML checkbox (<c>&lt;input type="checkbox"&gt;</c>).
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeCheckBox()</c> factory to create a model-bound checkbox with
    /// label, validation, and reactive event support.
    /// </remarks>
    public sealed class NativeCheckBox : NativeComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Checked = Property("checked");
        internal static readonly CapabilityMethod Focus = Method("focus");
        internal static readonly ComponentMetadata Definition = DescribeBindable("checkbox", Checked);
        internal override ComponentMetadata Metadata => Definition;
    }
}
