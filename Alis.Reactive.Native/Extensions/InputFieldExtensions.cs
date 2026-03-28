using System;
using System.Linq.Expressions;
using Alis.Reactive.InputField;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Razor view extension for starting a model-bound input field.
    /// </summary>
    public static class InputFieldExtensions
    {
        /// <summary>
        /// Starts a model-bound input field for <paramref name="expression"/>, with optional
        /// label and required marker.
        /// </summary>
        /// <remarks>
        /// Chain a component extension on the result to choose what renders inside the field —
        /// e.g. <c>.NativeTextBox()</c>, <c>.FusionDropDownList()</c>. The field wrapper handles
        /// label display and validation error placement automatically.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The model property type the field is bound to.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The plan this field belongs to.</param>
        /// <param name="expression">The model property to bind the field to.</param>
        /// <param name="options">Optional configuration for label text and required marker.</param>
        /// <returns>A bound field ready to receive a component extension.</returns>
        public static InputBoundField<TModel, TProp> InputField<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            Action<InputFieldOptions>? options = null)
            where TModel : class
        {
            var opts = new InputFieldOptions();
            options?.Invoke(opts);
            return new InputBoundField<TModel, TProp>(
                html,
                plan,
                expression,
                opts,
                IdGenerator.For(expression),
                html.NameFor(expression),
                html.ViewContext.Writer);
        }
    }
}
