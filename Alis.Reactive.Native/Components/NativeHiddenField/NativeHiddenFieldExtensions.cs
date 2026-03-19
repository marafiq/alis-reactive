using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeHiddenFieldExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        public static ComponentRef<NativeHiddenField, TModel> SetValue<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeHiddenField, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
