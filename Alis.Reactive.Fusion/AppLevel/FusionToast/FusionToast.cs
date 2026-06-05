namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// App-level Syncfusion Toast notification component.
    /// </summary>
    /// <remarks>
    /// One toast exists per page. Because it implements <see cref="IAppLevelComponent"/>,
    /// you can reference it without an explicit ID:
    /// <code>p.Component&lt;FusionToast&gt;().SetContent("Saved").Success().Show()</code>
    /// </remarks>
    public sealed class FusionToast : FusionComponent, IAppLevelComponent
    {
        /// <summary>
        /// The well-known element ID used by the toast in the layout.
        /// </summary>
        public const string ElementId = "alisFusionToast";

        /// <inheritdoc />
        public string DefaultId => ElementId;
    }
}
