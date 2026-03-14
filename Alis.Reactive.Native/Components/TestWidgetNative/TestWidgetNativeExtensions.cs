using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Native.Components
{
    public static class TestWidgetNativeExtensions
    {
        public static ComponentRef<TestWidgetNative, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self, string value)
            where TModel : class => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<TestWidgetNative, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self)
            where TModel : class => self.Emit(new CallMutation("focus"));
    }
}
