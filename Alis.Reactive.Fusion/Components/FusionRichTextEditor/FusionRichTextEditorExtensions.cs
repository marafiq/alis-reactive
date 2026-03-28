using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionRichTextEditor"/> in a reactive pipeline.
    /// </summary>
    public static class FusionRichTextEditorExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        /// <summary>Sets the HTML content value.</summary>
        /// <param name="value">The HTML content to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionRichTextEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>Moves focus into the rich text editor.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionRichTextEditor, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Reads the current HTML content for use in conditions or gather.</summary>
        /// <returns>A typed source representing the editor's current HTML content.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
