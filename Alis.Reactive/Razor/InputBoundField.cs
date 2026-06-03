using System;
using System.IO;
using System.Text.Encodings.Web;
using Alis.Reactive.InputField;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

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
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model property type the field is bound to.</typeparam>
    public class InputBoundField<TModel, TProp>
#if NET48
        : InputBoundFieldBase<HtmlHelper<TModel>, TModel, TProp>
#else
        : InputBoundFieldBase<IHtmlHelper<TModel>, TModel, TProp>
#endif
        where TModel : class
    {
        /// <summary>
        /// Keep internal: <see cref="Extensions.InputFieldExtensions.InputField{TModel,TProp}"/>
        /// creates the field and wires plan registration. Public construction would bypass
        /// validation and gather registration.
        /// </summary>
        internal InputBoundField(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            BoundInputField<TModel, TProp> field,
            TextWriter writer)
            : base(html, field, writer)
        {
        }

        /// <summary>
        /// Keep internal: component extensions use this to write markup inside the field
        /// wrapper after registration. Public rendering would bypass the component pipeline.
        /// </summary>
        /// <param name="content">The component markup to render inside the field wrapper.</param>
#if NET48
        internal void Render(IHtmlString content)
        {
            Render(() => Writer.Write(content.ToHtmlString()));
        }
#else
        internal void Render(IHtmlContent content)
        {
            Render(() => content.WriteTo(Writer, HtmlEncoder.Default));
        }
#endif
    }
}
