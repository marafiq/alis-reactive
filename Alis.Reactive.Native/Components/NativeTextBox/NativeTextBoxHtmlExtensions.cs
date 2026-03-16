using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extensions for creating NativeTextBoxBuilder.
    /// </summary>
    public static class NativeTextBoxHtmlExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        public static NativeTextBoxBuilder<TModel, TProp> NativeTextBoxFor<TModel, TProp>(
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

            return new NativeTextBoxBuilder<TModel, TProp>(html, expression);
        }
    }
}
