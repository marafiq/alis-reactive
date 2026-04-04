using System;
using Alis.Reactive.Builders.Conditions;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionDatePicker"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDatePicker&gt;(m =&gt; m.BirthDate).SetValue(new DateTime(2000, 1, 1))</c>.
    /// </remarks>
    public static class FusionDatePickerExtensions
    {
        private static readonly CapabilityMethod FocusInMethod = CapabilityMethod.Named("focusIn");
        private static readonly CapabilityMethod FocusOutMethod = CapabilityMethod.Named("focusOut");

        /// <summary>Sets the selected date.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The date to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self, DateTime value)
            where TModel : class
        {
            return self.Set(FusionDatePicker.Value, value);
        }

        /// <summary>Moves focus into the date picker.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.Call(FocusInMethod);

        /// <summary>Removes focus from the date picker.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDatePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.Call(FocusOutMethod);

        /// <summary>Reads the current date value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionDatePicker&gt;(m =&gt; m.BirthDate).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the date picker's current value.</returns>
        public static ReactiveValue<DateTime> Value<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.CreateValue<DateTime>();
    }
}
