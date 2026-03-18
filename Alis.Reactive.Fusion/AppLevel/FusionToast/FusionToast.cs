namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// App-level toast notification backed by Syncfusion Toast.
    /// Singleton per page — one SF Toast instance serves all notifications.
    ///
    /// Implements IAppLevelComponent so it can be resolved without an explicit ID:
    ///   p.Component&lt;FusionToast&gt;().SetContent("Saved").Success().Show()
    /// </summary>
    public sealed class FusionToast : FusionComponent, IAppLevelComponent
    {
        public const string ElementId = "alisFusionToast";

        public string DefaultId => ElementId;
    }
}
