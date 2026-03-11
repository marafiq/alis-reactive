using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Provides AsNumericTextBoxFor — collision-free ID variant of NumericTextBoxFor.
    /// Uses IdGenerator to produce a unique element ID while preserving the model binding name.
    /// </summary>
    public static class FusionNumericTextBoxIdExtensions
    {
        public static NumericTextBoxBuilder AsNumericTextBoxFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression).ToString();

            return html.EJS().NumericTextBoxFor(expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
        }
    }
}
