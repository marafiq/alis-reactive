using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive.InputField;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Closes <c>THelper</c> to <c>IHtmlHelper&lt;TModel&gt;</c> for ASP.NET Core.
    /// For net48: one #if swaps to <c>HtmlHelper&lt;TModel&gt;</c>.
    /// This is the ONLY file that binds the framework to the InputField infrastructure.
    /// </summary>
    public class InputFieldSetup<TModel, TProp>
        : InputFieldSetup<IHtmlHelper<TModel>, TModel, TProp>
        where TModel : class
    {
        internal InputFieldSetup(
            IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            InputFieldOptions options,
            string elementId,
            string bindingPath,
            TextWriter writer)
            : base(html, plan, expression, options, elementId, bindingPath, writer)
        {
        }

        /// <summary>
        /// Renders IHtmlContent inside the field wrapper.
        /// All framework rendering noise (WriteTo, HtmlEncoder) is here — extensions just pass content.
        /// </summary>
        public void Render(IHtmlContent content)
        {
            Render(() => content.WriteTo(Writer, HtmlEncoder.Default));
        }
    }

    public static class InputFieldExtensions
    {
        public static InputFieldSetup<TModel, TProp> InputField<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression,
            Action<InputFieldOptions>? options = null)
            where TModel : class
        {
            var opts = new InputFieldOptions();
            options?.Invoke(opts);
            return new InputFieldSetup<TModel, TProp>(
                html,
                plan,
                expression,
                opts,
                IdGenerator.For<TModel, TProp>(expression),
                html.NameFor(expression),
                html.ViewContext.Writer);
        }
    }
}
