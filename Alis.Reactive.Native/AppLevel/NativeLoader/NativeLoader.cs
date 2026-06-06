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
        /// Well-known layout element ID for the loader.
        /// </summary>
        public const string ElementId = "alis-loader";

        /// <inheritdoc />
        public string DefaultId => ElementId;
    }
}
