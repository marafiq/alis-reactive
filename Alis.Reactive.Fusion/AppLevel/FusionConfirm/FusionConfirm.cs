namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// App-level confirm dialog backed by Syncfusion Dialog.
    /// Singleton per page — one SF Dialog instance serves all ConfirmGuard evaluations.
    ///
    /// Implements IAppLevelComponent so it can be resolved without an explicit ID:
    ///   p.Component&lt;FusionConfirm&gt;().Show()
    /// </summary>
    public sealed class FusionConfirm : FusionComponent, IAppLevelComponent
    {
        public const string ElementId = "alisConfirmDialog";

        public string DefaultId => ElementId;
    }
}
