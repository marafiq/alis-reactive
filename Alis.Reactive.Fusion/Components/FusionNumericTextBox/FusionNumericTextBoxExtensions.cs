using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionNumericTextBox"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        /// <summary>Sets the minimum allowed value.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
            => self.EmitSet("min", ValueProducer.Literal(min));

        /// <summary>Moves focus into the numeric input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the numeric input.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        /// <summary>Increments the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall("increment");

        /// <summary>Decrements the value by one step.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.EmitCall("decrement");
    }
}
