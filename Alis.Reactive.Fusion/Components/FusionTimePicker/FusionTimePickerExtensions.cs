using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionTimePicker"/> values from a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionTimePicker&gt;(m =&gt; m.CheckInTime).SetValue(new DateTime(1, 1, 1, 14, 30, 0))</c>.
    /// </remarks>
    public static class FusionTimePickerExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        private static readonly ComponentProperty<DateTime> ValueProperty =
            ComponentProperty<DateTime>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        /// <summary>Sets selected time.</summary>
        /// <param name="value">Selected time; only the time portion is used.</param>
        public static ComponentRef<FusionTimePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty,
                ValueExpression.LiteralRaw(value.ToString("HH:mm", CultureInfo.InvariantCulture), Shape.Date));
        }

        /// <summary>Moves focus into the time picker.</summary>
        public static ComponentRef<FusionTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the time picker.</summary>
        public static ComponentRef<FusionTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Reads time value for conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionTimePicker&gt;(m =&gt; m.CheckInTime).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
