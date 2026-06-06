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
    /// Model-bound input field returned by
    /// <see cref="Extensions.InputFieldExtensions.InputField{TModel,TProp}"/>, ready to receive
    /// a component extension that renders inside the field wrapper.
    /// </summary>
    /// <remarks>
    /// Component extensions choose the concrete rendered control. The field
    /// wrapper owns label markup and validation message placement.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">Model property type the field is bound to.</typeparam>
    public class InputBoundField<TModel, TProp>
#if NET48
        : InputBoundFieldBase<HtmlHelper<TModel>, TModel, TProp>
#else
        : InputBoundFieldBase<IHtmlHelper<TModel>, TModel, TProp>
#endif
        where TModel : class
    {
        // Internal because Html.InputField creates the slot used by validation and gather.
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

        // Component extensions render only after registration; public rendering would bypass the component pipeline.
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
