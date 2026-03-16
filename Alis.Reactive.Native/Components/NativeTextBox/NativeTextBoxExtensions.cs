using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeTextBoxExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        public static ComponentRef<NativeTextBox, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextBox, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<NativeTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
