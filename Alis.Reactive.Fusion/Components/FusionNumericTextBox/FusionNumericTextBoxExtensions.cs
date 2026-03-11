using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for FusionNumericTextBox.
    ///
    /// Fusion jsEmit convention (el.ej2_instances[0]):
    ///   Prop  → var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()
    ///   Call  → el.ej2_instances[0].focusIn()
    ///   Read  → ref:{id}.value
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        // ── Builder: collision-free ID variant of SF NumericTextBoxFor ──

        /// <summary>
        /// Creates a Syncfusion NumericTextBox bound to a model property.
        /// Uses IdGenerator to produce a unique element ID while preserving the model binding name.
        /// </summary>
        public static NumericTextBoxBuilder NumericTextBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression).ToString();

            return html.EJS().NumericTextBoxFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }

        // ── Prop: assigns value, coerces to Number, calls dataBind() ──

        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(
                "var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()",
                value.ToString(CultureInfo.InvariantCulture));
        }

        // ── Call: invokes focusIn() on the SF instance ──

        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit("el.ej2_instances[0].focusIn()");
        }

        // ── Read: returns BindExpr for the component's current value ──

        /// <summary>
        /// Returns the BindExpr for reading this component's current numeric value.
        /// The TS resolver resolves comp.value via evalRead (vendor-aware)
        /// </summary>
        public static string Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.value";
        }
    }
}
