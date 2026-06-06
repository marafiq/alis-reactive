using Alis.Reactive;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level slide-out drawer panel that can be referenced without an explicit ID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One drawer exists per page and can be referenced without an explicit ID:
    /// </para>
    /// <code>p.Component&lt;NativeDrawer&gt;().Open()</code>
    /// </remarks>
    public sealed class NativeDrawer : NativeComponent, IAppLevelComponent
    {
        /// <summary>
        /// The well-known element ID used by the drawer in the layout.
        /// </summary>
        public const string ElementId = "alis-drawer";

        /// <inheritdoc />
        public string DefaultId => ElementId;
    }
}
