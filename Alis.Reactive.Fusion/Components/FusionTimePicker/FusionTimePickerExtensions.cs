using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;

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
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The time to set (only the time portion is used).</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Set("value", value.ToString("HH:mm", CultureInfo.InvariantCulture));
        }

        /// <summary>Moves focus into the time picker.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Call("focusIn");

        /// <summary>Removes focus from the time picker.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Call("focusOut");

        /// <summary>Reads the current time value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionTimePicker&gt;(m =&gt; m.CheckInTime).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the time picker's current value.</returns>
        public static ComponentValueExpression<DateTime> Value<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => new ComponentValueExpression<DateTime>(self.TargetId, Component.Vendor, Component.ValueMemberPath);
    }
}
