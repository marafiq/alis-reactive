using Alis.Reactive;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level slide-out drawer backed by native DOM.
    /// Singleton per page — one drawer element serves all open/close interactions.
    ///
    /// Implements IAppLevelComponent so it can be resolved without an explicit ID:
    ///   p.Component&lt;NativeDrawer&gt;().Open()
    /// </summary>
    public sealed class NativeDrawer : NativeComponent, IAppLevelComponent
    {
        public const string ElementId = "alis-drawer";

        public string DefaultId => ElementId;
    }
}
