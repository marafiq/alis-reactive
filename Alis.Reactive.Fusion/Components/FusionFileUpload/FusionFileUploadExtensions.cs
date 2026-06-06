using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads selected file metadata from <see cref="FusionFileUpload"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Document).Value()</c>.
    /// </para>
    /// <para>
    /// No <c>SetValue()</c> is provided. Files are selected by user interaction and submitted through FormData.
    /// </para>
    /// </remarks>
    public static class FusionFileUploadExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        /// <summary>Reads selected file metadata for conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Document).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionFileUpload, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
