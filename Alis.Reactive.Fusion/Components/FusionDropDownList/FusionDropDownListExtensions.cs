using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for FusionDropDownList.
    ///
    /// Fusion jsEmit convention (el = vendor-resolved root, i.e. the ej2 instance):
    ///   Prop write → el.value=val
    ///   Prop read  → ReadProperty&lt;string&gt;("value") → TypedComponentSource
    ///   Call        → el.focusIn()
    /// </summary>
    public static class FusionDropDownListExtensions
    {
        private static readonly FusionDropDownList _component = new FusionDropDownList();

        // ── Builder: collision-free ID variant of SF DropDownListFor ──

        /// <summary>
        /// Creates a Syncfusion DropDownList bound to a model property.
        /// Uses IdGenerator to produce a unique element ID while preserving the model binding name.
        /// </summary>
        public static DropDownListBuilder DropDownListFor<TModel, TProp>(
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

            return html.EJS().DropDownListFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }

        // ── Prop writes ──

        /// <summary>Sets the selected value on the SF instance.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string? value)
            where TModel : class
        {
            return self.Emit("el.value=val", value);
        }

        /// <summary>Sets the display text on the SF instance.</summary>
        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
        {
            return self.Emit("el.text=val", text);
        }

        // ── Method calls (void, no args) ──

        /// <summary>Invokes focusIn() on the SF instance.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.Emit("el.focusIn()");
        }

        /// <summary>Invokes focusOut() on the SF instance.</summary>
        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.Emit("el.focusOut()");
        }

        /// <summary>Shows the popup list.</summary>
        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.Emit("el.showPopup()");
        }

        /// <summary>Hides the popup list.</summary>
        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.Emit("el.hidePopup()");
        }

        // ── Prop reads (return TypedComponentSource for use in conditions/bindings) ──

        /// <summary>
        /// Returns a TypedComponentSource for reading this component's selected value.
        /// Use in conditions: p.When(comp.Value()).Eq("US")
        /// Use in bindings: p.Element("echo").SetText(comp.Value())
        /// </summary>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.ReadProperty<string>("value");
        }

        /// <summary>
        /// Returns a TypedComponentSource for reading this component's display text.
        /// Use in conditions: p.When(comp.Text()).Eq("United States")
        /// </summary>
        public static TypedComponentSource<string> Text<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
        {
            return self.ReadProperty<string>("text");
        }
    }
}
