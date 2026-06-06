namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// App-level confirm dialog backed by Syncfusion Dialog that can be referenced without an explicit ID.
    /// </summary>
    /// <remarks>
    /// One Syncfusion Dialog instance serves all confirm condition evaluations
    /// and can be referenced without an explicit ID:
    /// <code>p.Component&lt;FusionConfirm&gt;().Show()</code>
    /// </remarks>
    public sealed class FusionConfirm : FusionComponent, IAppLevelComponent
    {
        public const string ElementId = "alisConfirmDialog";

        public string DefaultId => ElementId;
    }
}
