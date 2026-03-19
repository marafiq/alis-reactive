using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeTextAreaExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static ComponentRef<NativeTextArea, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextArea, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
