using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML radio button group.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeRadioGroup()</c> factory to create a model-bound radio group with
    /// label, validation, and reactive event support. A hidden input holds the
    /// selected value for form submission and component reads.
    /// </remarks>
    public sealed class NativeRadioGroup : NativeComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Value = Property("value");
        internal static readonly CapabilityMethod Focus = Method("focus");
        internal static readonly ComponentMetadata Definition = DescribeBindable("radiogroup", Value);
        internal override ComponentMetadata Metadata => Definition;
    }
}
