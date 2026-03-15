using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDatePickerExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static ComponentRef<NativeDatePicker, TModel> SetValue<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        public static ComponentRef<NativeDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<DateTime>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
