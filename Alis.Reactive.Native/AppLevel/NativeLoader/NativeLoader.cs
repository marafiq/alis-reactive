namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// App-level loading overlay that covers its target container or the viewport and can be referenced without an explicit ID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One loader exists per page and can be referenced without an explicit ID:
    /// </para>
    /// <code>p.Component&lt;NativeLoader&gt;().Show()</code>
    /// </remarks>
    public sealed class NativeLoader : NativeComponent, IAppLevelComponent
    {
        /// <summary>
        /// The well-known element ID used by the loader in the layout.
        /// </summary>
        public const string ElementId = "alis-loader";

        /// <inheritdoc />
        public string DefaultId => ElementId;
    }
}
