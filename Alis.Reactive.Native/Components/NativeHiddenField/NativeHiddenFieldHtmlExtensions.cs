using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeHiddenFieldBuilder.
    /// Hidden fields bypass InputField wrapper entirely — no label, no validation slot.
    /// Registers in RegisteredComponents for gather (IncludeAll picks it up).
    /// </summary>
    public static class NativeHiddenFieldHtmlExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        /// <summary>
        /// Creates a hidden field bound to the model property.
        /// Registers in RegisteredComponents for gather — no InputField wrapper.
        /// Returns IHtmlContent for direct rendering in views: @Html.HiddenFieldFor(plan, m => m.Id)
        /// </summary>
        public static NativeHiddenFieldBuilder<TModel, TProp> HiddenFieldFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var elementId = IdGenerator.For<TModel, TProp>(expression);
            var bindingPath = html.NameFor(expression);

            plan.RegisterComponent(bindingPath, new ComponentRegistration(
                elementId, ReactiveComponentMetadata.For(_component), bindingPath,
                ValueShapeFactory.FromClrType(typeof(TProp))));

            return new NativeHiddenFieldBuilder<TModel, TProp>(html, expression);
        }
    }
}
