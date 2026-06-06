namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// App-level confirm dialog backed by Syncfusion Dialog.
    /// Singleton per page — one Syncfusion Dialog instance serves all confirm condition evaluations.
    ///
    /// Implements IAppLevelComponent so it can be resolved without an explicit ID,
    /// for example <c>p.Component&lt;FusionConfirm&gt;().Show()</c>.
    /// </summary>
    public sealed class FusionConfirm : FusionComponent, IAppLevelComponent
    {
        public const string ElementId = "alisConfirmDialog";

        public string DefaultId => ElementId;
    }
}
