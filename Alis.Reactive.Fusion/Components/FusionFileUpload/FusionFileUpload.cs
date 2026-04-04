using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionFileUpload for selecting files in form mode (no auto-upload).
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Documents)</c>
    /// to access FusionFileUpload-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionFileUpload : FusionComponent, IBindableComponent
    {
        internal static readonly CapabilityProperty Files = Property("files", PathSegment.FromProp("filesData"));
        internal static readonly ComponentMetadata Definition = DescribeBindable("fileupload", Files);
        internal override ComponentMetadata Metadata => Definition;
    }
}
