using Alis.Reactive;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDropDownExtensions
    {
        public static ComponentRef<NativeDropDown, TModel> SetValue<TModel>(
            this ComponentRef<NativeDropDown, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<NativeDropDown, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallVoidMutation("focus"));
        }

        public static string Value<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.value";
        }
    }
}
