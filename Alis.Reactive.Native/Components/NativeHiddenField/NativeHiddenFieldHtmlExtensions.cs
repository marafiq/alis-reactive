using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var bindingPath = html.NameFor(expression);
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
