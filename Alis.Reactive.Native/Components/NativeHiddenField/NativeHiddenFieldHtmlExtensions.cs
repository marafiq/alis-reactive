using System;
using System.Linq.Expressions;
using Alis.Reactive.Native.Extensions;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for rendering model-bound hidden fields directly in a Razor view.
    /// </summary>
    /// <remarks>
    /// A hidden field is a hidden <c>InputField</c>: it renders without the label and validation
    /// slot, and still registers with the Reactive Plan for gather.
    /// </remarks>
    public static class NativeHiddenFieldHtmlExtensions
    {
        /// <summary>
        /// Registers and renders a hidden field bound to a model property.
        /// </summary>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model value type registered as the hidden input value.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan that receives the hidden-field registration.</param>
        /// <param name="expression">Model property expression used for MVC binding and component registration.</param>
        /// <returns>The rendered hidden field, ready for <c>.Reactive(...)</c>.</returns>
        public static RenderedComponent<NativeHiddenField, TModel> HiddenFieldFor<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var field = html.InputField(plan, expression, o => o.Hidden());
            return field.Render<NativeHiddenField>(
                new NativeHiddenFieldBuilder<TModel, TProp>(html, expression, field.ElementId, field.BindingPath));
        }
    }
}
