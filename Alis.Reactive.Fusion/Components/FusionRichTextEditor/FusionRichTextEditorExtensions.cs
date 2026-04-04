using Alis.Reactive.Builders.Conditions;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionRichTextEditor"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.Notes).SetValue("&lt;p&gt;Hello&lt;/p&gt;")</c>.
    /// </remarks>
    public static class FusionRichTextEditorExtensions
    {
        private static readonly CapabilityMethod FocusInMethod = CapabilityMethod.Named("focusIn");

        /// <summary>Sets the HTML content value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The HTML content to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionRichTextEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self, string value)
            where TModel : class
        {
            return self.Set(FusionRichTextEditor.Value, value);
        }

        /// <summary>Moves focus into the rich text editor.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionRichTextEditor, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.Call(FocusInMethod);

        /// <summary>Reads the current HTML content for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.Notes).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the editor's current HTML content.</returns>
        public static ReactiveValue<string> Value<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.CreateValue<string>();
    }
}
