using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionDateTimePicker (SetValue, FocusIn, FocusOut, Value).
    /// </summary>
    public static class FusionDateTimePickerExtensions
    {
        private static readonly FusionDateTimePicker Component = new FusionDateTimePicker();

        public static ComponentRef<FusionDateTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionDateTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionDateTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
