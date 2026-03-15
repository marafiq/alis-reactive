using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionDropDownListExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        public static DropDownListBuilder DropDownListFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For(expression);
            var name = html.NameFor(expression);

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                Component.Vendor,
                name,
                Component.ReadExpr));

            return html.EJS().DropDownListFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }

        public static ComponentRef<FusionDropDownList, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
