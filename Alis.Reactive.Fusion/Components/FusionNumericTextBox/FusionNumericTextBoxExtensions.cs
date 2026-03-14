using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for FusionNumericTextBox.
    ///
    /// Structured mutation convention (root = vendor-resolved ej2 instance):
    ///   Prop write → { prop: "value", coerce: "number" }
    ///   Prop read  → ReadProperty&lt;decimal&gt;("value") → TypedComponentSource
    ///   Call        → { method: "focusIn" }
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        private static readonly FusionNumericTextBox _component = new FusionNumericTextBox();

        // ── Builder: collision-free ID variant of SF NumericTextBoxFor ──

        /// <summary>
        /// Creates a Syncfusion NumericTextBox bound to a model property.
        /// Uses IdGenerator to produce a unique element ID while preserving the model binding name.
        /// </summary>
        public static NumericTextBoxBuilder NumericTextBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression).ToString();

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return html.EJS().NumericTextBoxFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }

        // ── Prop writes ──

        /// <summary>Sets the numeric value on the SF instance.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(
                prop: "value",
                value: value.ToString(CultureInfo.InvariantCulture),
                coerce: "number");
        }

        /// <summary>Sets the minimum allowed value on the SF instance.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.Emit(
                prop: "min",
                value: min.ToString(CultureInfo.InvariantCulture),
                coerce: "number");
        }

        // ── Method calls (void, no args) ──

        /// <summary>Invokes focusIn() on the SF instance.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(method: "focusIn");
        }

        /// <summary>Invokes focusOut() on the SF instance.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(method: "focusOut");
        }

        /// <summary>Increments the numeric value by the step amount.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(method: "increment");
        }

        /// <summary>Decrements the numeric value by the step amount.</summary>
        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(method: "decrement");
        }

        // ── Prop reads (return TypedComponentSource for use in conditions/bindings) ──

        /// <summary>
        /// Returns a TypedComponentSource for reading this component's current numeric value.
        /// Use in conditions: p.When(comp.Value()).Gte(100m)
        /// Use in bindings: p.Element("echo").SetText(comp.Value())
        /// </summary>
        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.ReadProperty<decimal>("value");
        }

        /// <summary>
        /// Returns a TypedComponentSource for reading this component's min property.
        /// Use in conditions: p.When(comp.Min()).Gte(0m)
        /// </summary>
        public static TypedComponentSource<decimal> Min<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.ReadProperty<decimal>("min");
        }
    }
}
