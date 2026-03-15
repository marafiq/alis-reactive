using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionTimePicker (SetValue, Value).
    /// </summary>
    public static class FusionTimePickerExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        public static ComponentRef<FusionTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("HH:mm", CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
