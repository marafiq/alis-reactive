using System;
using System.Globalization;
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
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        // SetValue&lt;TProp&gt;(TProp value) is provided by the ComponentRef base class.

        /// <summary>Moves focus into the date picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the date picker.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDatePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDatePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        // Value&lt;TProp&gt;() is provided by the ComponentRef base class.
    }
}
