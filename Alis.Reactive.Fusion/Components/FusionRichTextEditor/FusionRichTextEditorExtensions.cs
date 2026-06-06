using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionRichTextEditor"/> in a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.Notes).SetValue("&lt;p&gt;Hello&lt;/p&gt;")</c>.
    /// </remarks>
    public static class FusionRichTextEditorExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets the HTML content value.</summary>
        /// <param name="value">HTML content to set.</param>
        public static ComponentRef<FusionRichTextEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>Moves focus into the rich text editor.</summary>
        public static ComponentRef<FusionRichTextEditor, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads the HTML content for conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.Notes).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
