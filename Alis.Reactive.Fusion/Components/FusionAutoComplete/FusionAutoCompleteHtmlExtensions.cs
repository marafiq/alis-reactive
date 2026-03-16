using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating AutoCompleteBuilder bound to a model property.
    /// SF EJ2 ASP.NET Core exposes the AutoComplete concept via AutoCompleteBuilder.
    /// </summary>
    public static class FusionAutoCompleteHtmlExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        public static AutoCompleteBuilder AutoCompleteFor<TModel, TProp>(
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

            return html.EJS().AutoCompleteFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }
    }
}
