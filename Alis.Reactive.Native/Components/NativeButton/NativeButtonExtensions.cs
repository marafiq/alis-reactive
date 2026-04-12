using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    public static class NativeButtonExtensions
    {
        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.EmitSet("textContent", ValueProducer.Literal(text));
        }

        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.EmitCall("focus");
        }
    }
}
