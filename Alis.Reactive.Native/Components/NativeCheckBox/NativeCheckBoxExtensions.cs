using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeCheckBoxExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        public static ComponentRef<NativeCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("checked", coerce: "boolean"), value: isChecked ? "true" : "false");
        }

        public static ComponentRef<NativeCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<bool> Value<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<bool>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
