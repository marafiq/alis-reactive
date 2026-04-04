using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level slide-out drawer panel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One drawer exists per page. Because it implements <see cref="IAppLevelComponent"/>,
    /// you can reference it without an explicit ID:
    /// </para>
    /// <code>p.Component&lt;NativeDrawer&gt;().Open()</code>
    /// </remarks>
    public sealed class NativeDrawer : NativeComponent, IAppLevelComponent
    {
        internal static readonly CapabilityMethod AddCssClass =
            Method("addCssClass", PathSegment.FromProp("classList"), PathSegment.FromProp("add"));
        internal static readonly CapabilityMethod RemoveCssClass =
            Method("removeCssClass", PathSegment.FromProp("classList"), PathSegment.FromProp("remove"));
        internal static readonly CapabilityMethod RemoveAttribute = Method("removeAttribute");
        internal static readonly ComponentMetadata Definition = Describe("drawer");

        /// <summary>
        /// The well-known element ID used by the drawer in the layout.
        /// </summary>
        public const string ElementId = "alis-drawer";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        internal override ComponentMetadata Metadata => Definition;
    }
}
