namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Represents the page-level toast notification component.
    /// </summary>
    public sealed class FusionToast : FusionComponent, IAppLevelComponent
    {
        internal static readonly ComponentMetadata Definition = Describe("toast");

        /// <summary>Gets the DOM element id used for the singleton toast host.</summary>
        public const string ElementId = "alisFusionToast";

        /// <summary>Gets the default component id used when resolving the app-level toast.</summary>
        public string DefaultId => ElementId;

        internal override ComponentMetadata Metadata => Definition;
    }
}
