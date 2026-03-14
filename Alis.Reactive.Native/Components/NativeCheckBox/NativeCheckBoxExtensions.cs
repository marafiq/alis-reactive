using Alis.Reactive;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Native.Components
{
    public static class NativeCheckBoxExtensions
    {
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
            return self.Emit(new CallVoidMutation("focus"));
        }

        public static string Checked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.checked";
        }
    }
}
