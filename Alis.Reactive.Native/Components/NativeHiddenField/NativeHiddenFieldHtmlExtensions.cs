using System;
using System.Linq.Expressions;
#if NET48
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

using ComponentRegistrationSource = Alis.Reactive.Native.Components.NativeHiddenField;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for rendering model-bound hidden fields directly in a Razor view.
    /// </summary>
    /// <remarks>
    /// Hidden fields bypass the <c>InputField</c> wrapper because they do not render
    /// labels or validation slots, but they still register with the Reactive Plan for gather.
    /// </remarks>
    public static class NativeHiddenFieldHtmlExtensions
    {
        /// <summary>
        /// Registers and renders a hidden field bound to a model property.
        /// </summary>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model value type registered as the hidden input value.</typeparam>
        /// <param name="plan">Reactive Plan that receives the hidden-field registration.</param>
        /// <param name="expression">Model property expression used for MVC binding and component registration.</param>
        /// <returns>Hidden input builder for inline rendering.</returns>
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
            var registration = ComponentRegistrationSource.Registration;
            var slot = ModelBoundInputComponentSlot.For<TModel, TProp>(expression, bindingPath);
            plan.RegisterInputComponent(slot.Register(registration));

            return new NativeHiddenFieldBuilder<TModel, TProp>(
                html,
                expression,
                slot.RenderTarget);
        }
    }
}
