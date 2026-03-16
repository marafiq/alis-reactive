using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeDropDownBuilder bound to a model property.
    /// </summary>
    public static class NativeDropDownHtmlExtensions
    {
        private static readonly NativeDropDown _component = new NativeDropDown();

        /// <summary>
        /// Creates a native &lt;select&gt; builder bound to a model property.
        /// </summary>
        public static NativeDropDownBuilder<TModel, TProp> NativeDropDownFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression);

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return new NativeDropDownBuilder<TModel, TProp>(html, expression);
        }
    }
}
