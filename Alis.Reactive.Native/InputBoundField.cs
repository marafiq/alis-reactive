using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive.InputField;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native
{
    /// <summary>
    /// A model-bound input field returned by
    /// <see cref="Extensions.InputFieldExtensions.InputField{TModel,TProp}"/>, ready to receive
    /// a component extension that renders inside the field wrapper.
    /// </summary>
    /// <remarks>
    /// Chain a component extension on this to choose what renders — e.g.
    /// <c>.NativeTextBox()</c>, <c>.FusionDropDownList()</c>. The field wrapper provides
    /// the label and validation error HTML elements automatically.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The model property type the field is bound to.</typeparam>
    public class InputBoundField<TModel, TProp>
        : InputBoundFieldBase<IHtmlHelper<TModel>, TModel, TProp>
        where TModel : class
    {
        /// <summary>
        /// NEVER make public. Devs get this from <see cref="Extensions.InputFieldExtensions.InputField{TModel,TProp}"/>
        /// — a public constructor would bypass plan registration and break validation and gather.
        /// </summary>
        internal InputBoundField(
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
        /// NEVER make public. Component extensions call this to write their markup inside the
        /// field wrapper — exposing it lets devs bypass the component pipeline entirely.
        /// </summary>
        /// <param name="content">The component markup to render inside the field wrapper.</param>
        internal void Render(IHtmlContent content)
        {
            Render(() => content.WriteTo(Writer, HtmlEncoder.Default));
        }
    }
}
