using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionRichTextEditor (SetValue, FocusIn, Value).
    /// </summary>
    public static class FusionRichTextEditorExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        public static ComponentRef<FusionRichTextEditor, TModel> SetValue<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<FusionRichTextEditor, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
