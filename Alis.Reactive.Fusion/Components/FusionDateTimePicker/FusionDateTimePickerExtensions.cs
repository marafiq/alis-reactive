using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

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

        private static readonly ComponentProperty<DateTime> ValueProperty =
            ComponentProperty<DateTime>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        /// <summary>Sets the selected date and time.</summary>
        /// <param name="value">The date-time to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty,
                ValueExpression.LiteralRaw(value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture), Shape.Date));
        }

        /// <summary>Moves focus into the date-time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the date-time picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Reads the current date-time value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionDateTimePicker&gt;(m =&gt; m.AppointmentTime).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the date-time picker's current value.</returns>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
