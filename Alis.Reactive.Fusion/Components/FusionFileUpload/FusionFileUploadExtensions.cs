using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed value reading for <see cref="FusionFileUpload"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Document).Value()</c>.
    /// </para>
    /// <para>
    /// No <c>SetValue()</c> is provided. Files are set by user interaction only.
    /// </para>
    /// </remarks>
    public static class FusionFileUploadExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        /// <summary>Reads the current file data for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Document).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the uploader's current file data.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionFileUpload, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
