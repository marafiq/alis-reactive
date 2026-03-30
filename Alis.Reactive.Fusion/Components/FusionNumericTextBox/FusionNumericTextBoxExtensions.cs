using System.Globalization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

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
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        /// <summary>Sets the numeric value.</summary>
        /// <param name="value">The number to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value", coerce: "number"),
                value: value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Sets the minimum allowed value.</summary>
        /// <param name="min">The minimum value.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("min", coerce: "number"),
                value: min.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Moves focus into the numeric input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Removes focus from the numeric input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        /// <summary>Increments the value by one step.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("increment"));

        /// <summary>Decrements the value by one step.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("decrement"));

        /// <summary>Reads the current numeric value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Quantity).Value()).Gt(0m).Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the numeric input's current value.</returns>
        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => new TypedComponentSource<decimal>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
