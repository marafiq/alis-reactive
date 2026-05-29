using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    public static class NativeButtonExtensions
    {
        private static readonly ComponentProperty<string> TextContentProperty =
            ComponentProperty<string>.Named("textContent");

        private static readonly ComponentMethod FocusMethod =
            ComponentMethod.Named("focus");

        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.EmitSet(TextContentProperty, ValueExpression.Literal(text));
        }

        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.EmitCall(FocusMethod);
        }
    }
}
