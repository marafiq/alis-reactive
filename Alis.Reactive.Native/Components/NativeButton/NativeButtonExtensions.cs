using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Values;

namespace Alis.Reactive.Native.Components
{
    public static class NativeButtonExtensions
    {
        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("textContent", CommandValue.FromLiteral(text)));
        }

        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }
    }
}
