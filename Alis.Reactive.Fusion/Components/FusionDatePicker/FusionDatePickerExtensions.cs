using System;
using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reads and updates <see cref="FusionDatePicker"/> values from a Reactive Plan pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.BirthDate).SetValue(new DateTime(2000, 1, 1))</c>.
    /// </remarks>
    public static class FusionDatePickerExtensions
    {
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        private static readonly ComponentProperty<DateTime> ValueProperty =
            ComponentProperty<DateTime>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        /// <summary>Sets selected date.</summary>
        /// <param name="value">Selected date.</param>
        public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty,
                ValueExpression.LiteralRaw(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Shape.Date));
        }

        /// <summary>Moves focus into the date picker.</summary>
        public static ComponentRef<FusionDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the date picker.</summary>
        public static ComponentRef<FusionDatePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Reads date value for conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionDatePicker&gt;(m =&gt; m.BirthDate).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<DateTime> Value<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
