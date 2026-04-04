using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level loading overlay that covers its target container or the viewport.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One loader exists per page. Because it implements <see cref="IAppLevelComponent"/>,
    /// you can reference it without an explicit ID:
    /// </para>
    /// <code>p.Component&lt;NativeLoader&gt;().Show()</code>
    /// </remarks>
    public sealed class NativeLoader : NativeComponent, IAppLevelComponent
    {
        internal static readonly CapabilityMethod SetAttribute = Method("setAttribute");
        internal static readonly CapabilityMethod RemoveAttribute = Method("removeAttribute");
        internal static readonly CapabilityMethod AddCssClass =
            Method("addCssClass", PathSegment.FromProp("classList"), PathSegment.FromProp("add"));
        internal static readonly CapabilityMethod RemoveCssClass =
            Method("removeCssClass", PathSegment.FromProp("classList"), PathSegment.FromProp("remove"));
        internal static readonly ComponentMetadata Definition = Describe("loader");

        /// <summary>
        /// The well-known element ID used by the loader in the layout.
        /// </summary>
        public const string ElementId = "alis-loader";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        internal override ComponentMetadata Metadata => Definition;
    }
}
