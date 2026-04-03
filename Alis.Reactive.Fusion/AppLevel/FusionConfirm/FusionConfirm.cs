namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Represents the page-level confirmation dialog component.
    /// </summary>
    public sealed class FusionConfirm : FusionComponent, IAppLevelComponent
    {
        /// <summary>Gets the DOM element id used for the singleton confirm host.</summary>
        public const string ElementId = "alisConfirmDialog";

        /// <summary>Gets the default component id used when resolving the app-level dialog.</summary>
        public string DefaultId => ElementId;
    }
}
