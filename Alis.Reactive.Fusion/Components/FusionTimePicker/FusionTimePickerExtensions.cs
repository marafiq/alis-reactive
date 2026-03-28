using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionTimePicker"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionTimePicker&gt;(m =&gt; m.CheckInTime).SetValue(new DateTime(1, 1, 1, 14, 30, 0))</c>.
    /// </remarks>
    public static class FusionTimePickerExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        /// <summary>Sets the selected time.</summary>
        /// <param name="value">The time to set (only the time portion is used).</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("HH:mm", CultureInfo.InvariantCulture));
        }

        /// <summary>Moves focus into the time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Removes focus from the time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        /// <summary>Reads the current time value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the time picker's current value.</returns>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
