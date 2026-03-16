using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extensions for creating NativeDatePickerBuilder.
    /// </summary>
    public static class NativeDatePickerHtmlExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static NativeDatePickerBuilder<TModel, TProp> NativeDatePickerFor<TModel, TProp>(
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

            return new NativeDatePickerBuilder<TModel, TProp>(html, expression);
        }
    }
}
