namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level loading overlay. Singleton per page.
    /// Covers the nearest positioned container (or viewport by default).
    ///
    /// Usage: p.Component&lt;NativeLoader&gt;().Show()
    /// </summary>
    public sealed class NativeLoader : NativeComponent, IAppLevelComponent
    {
        public const string ElementId = "alis-loader";

        public string DefaultId => ElementId;
    }
}
