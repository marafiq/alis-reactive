using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionNumericTextBoxExtensions
    {
        private static readonly FusionNumericTextBox _component = new FusionNumericTextBox();

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

        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value", coerce: "number"),
                value: value.ToString(CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("min", coerce: "number"),
                value: min.ToString(CultureInfo.InvariantCulture));
        }

        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("increment"));

        public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("decrement"));

        public static TypedComponentSource<decimal> Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
            => new TypedComponentSource<decimal>(self.TargetId, _component.Vendor, _component.ReadExpr);
    }
}
