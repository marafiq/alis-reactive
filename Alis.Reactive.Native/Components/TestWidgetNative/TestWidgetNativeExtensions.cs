using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Values;

namespace Alis.Reactive.Native.Components
{
    public static class TestWidgetNativeExtensions
    {
        public static ComponentRef<TestWidgetNative, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self, string value)
            where TModel : class => self.Emit(new SetPropMutation("value", CommandValue.FromLiteral(value)));

        public static ComponentRef<TestWidgetNative, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self)
            where TModel : class => self.Emit(new CallMutation("focus"));
    }
}
