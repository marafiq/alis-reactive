using System.Globalization;
using Alis.Reactive.Builders.Conditions;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionNumericTextBox"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Quantity).SetValue(10m)</c>.
    /// </remarks>
    public static class FusionNumericTextBoxExtensions
    {
        private static readonly CapabilityProperty MinimumProperty = CapabilityProperty.Named("min");
        private static readonly CapabilityMethod FocusInMethod = CapabilityMethod.Named("focusIn");
        private static readonly CapabilityMethod FocusOutMethod = CapabilityMethod.Named("focusOut");
        private static readonly CapabilityMethod IncrementMethod = CapabilityMethod.Named("increment");
        private static readonly CapabilityMethod DecrementMethod = CapabilityMethod.Named("decrement");

        /// <summary>Sets the numeric value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The number to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Set(FusionNumericTextBox.Value, value);
        }

        /// <summary>Sets the minimum allowed value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="min">The minimum value.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.Set(MinimumProperty, min);
        }

        /// <summary>Moves focus into the numeric input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Call(FocusInMethod);

        /// <summary>Removes focus from the numeric input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Call(FocusOutMethod);

        /// <summary>Increments the value by one step.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Call(IncrementMethod);

        /// <summary>Decrements the value by one step.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Call(DecrementMethod);

        /// <summary>Reads the current numeric value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Quantity).Value()).Gt(0m).Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the numeric input's current value.</returns>
        public static ReactiveValue<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.CreateValue<decimal>();
    }
}
