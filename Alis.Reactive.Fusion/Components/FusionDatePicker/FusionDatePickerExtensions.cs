using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionDatePicker (SetValue, FocusIn, FocusOut, Value).
    /// </summary>
    public static class FusionDatePickerExtensions
    {
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionDatePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
