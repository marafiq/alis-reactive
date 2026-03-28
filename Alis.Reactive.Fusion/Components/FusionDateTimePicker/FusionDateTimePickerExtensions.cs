using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionDateTimePicker"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.AppointmentTime).SetValue(DateTime.Now)</c>.
    /// </remarks>
    public static class FusionDateTimePickerExtensions
    {
        private static readonly FusionDateTimePicker Component = new FusionDateTimePicker();

        /// <summary>Sets the selected date and time.</summary>
        /// <param name="value">The date-time to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"),
                value: value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture));
        }

        /// <summary>Moves focus into the date-time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Removes focus from the date-time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        /// <summary>Reads the current date-time value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the date-time picker's current value.</returns>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
