using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeHiddenFieldBuilder.
    /// Hidden fields bypass InputField wrapper entirely — no label, no validation slot.
    /// Registers in ComponentsMap for gather (IncludeAll picks it up).
    /// </summary>
    public static class NativeHiddenFieldHtmlExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        /// <summary>
        /// Creates a hidden field bound to the model property.
        /// Registers in ComponentsMap for gather — no InputField wrapper.
        /// Returns IHtmlContent for direct rendering in views: @Html.HiddenFieldFor(plan, m => m.Id)
        /// </summary>
        public static NativeHiddenFieldBuilder<TModel, TProp> HiddenFieldFor<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var elementId = IdGenerator.For<TModel, TProp>(expression);
#if NET48
            var bindingPath = ExpressionHelper.GetExpressionText(expression);
#else
            var bindingPath = html.NameFor(expression);
#endif

            plan.AddToComponentsMap(bindingPath, new ComponentRegistration(
                elementId, _component.Vendor, bindingPath, _component.ReadExpr, "hiddenfield"));

            return new NativeHiddenFieldBuilder<TModel, TProp>(html, expression);
        }
    }
}
