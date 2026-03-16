using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeCheckBoxBuilder bound to a model property.
    /// </summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        public static NativeCheckBoxBuilder<TModel, bool> NativeCheckBoxFor<TModel>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, bool>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, bool>(expression);
            var name = html.NameFor(expression);

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return new NativeCheckBoxBuilder<TModel, bool>(html, expression);
        }
    }
}
