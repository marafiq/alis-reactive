using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed component operations and value reads for <see cref="FusionNumericTextBox"/> in a Reactive Plan pipeline.
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        private static readonly ComponentProperty<decimal> ValueProperty =
            ComponentProperty<decimal>.Named(Component.ValueMember);

        private static readonly ComponentProperty<decimal> MinProperty =
            ComponentProperty<decimal>.Named("min");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        private static readonly ComponentMethod IncrementMethod =
            ComponentMethod.Named("increment");

        private static readonly ComponentMethod DecrementMethod =
            ComponentMethod.Named("decrement");

        /// <summary>Sets the numeric value.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>Sets the minimum allowed value.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.EmitSet(MinProperty, ValueExpression.Literal(min));
        }

        /// <summary>Moves focus into the numeric input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the numeric input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Increments the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(IncrementMethod);

        /// <summary>Decrements the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall(DecrementMethod);

        /// <summary>Reads the current numeric value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the numeric input's current value.</returns>
        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
