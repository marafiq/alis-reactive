using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
#if NET48
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeHiddenFieldBuilder.
    /// Hidden fields bypass InputField wrapper entirely -- no label, no validation slot.
    /// Registers in the input component onboarding catalog for gather.
    /// </summary>
    public static class NativeHiddenFieldHtmlExtensions
    {
        /// <summary>
        /// Creates a hidden field bound to the model property.
        /// Registers in the input component onboarding catalog for gather -- no InputField wrapper.
        /// Returns IHtmlContent for direct rendering in views: @Html.HiddenFieldFor(plan, m => m.Id)
        /// </summary>
        public static NativeHiddenFieldBuilder<TModel, TProp> HiddenFieldFor<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
#if NET48
            // System.Web.Mvc NameFor honors the active HtmlFieldPrefix; ExpressionHelper.GetExpressionText drops it.
            var bindingPath = html.NameFor(expression).ToHtmlString();
#else
            var bindingPath = html.NameFor(expression);
#endif
            var registration = global::Alis.Reactive.Native.Components.NativeHiddenField.Registration;
            var slot = ModelBoundInputComponentSlot.For<TModel, TProp>(expression, bindingPath);
            plan.RegisterInputComponent(slot.Register(registration));

            return new NativeHiddenFieldBuilder<TModel, TProp>(
                html,
                expression,
                slot.RenderTarget);
        }
    }
}
