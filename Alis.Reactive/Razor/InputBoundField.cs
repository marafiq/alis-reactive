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

#if NET48
        private static string MarkupOf(IHtmlString content) => content.ToHtmlString();

        /// <summary>
        /// Registers <typeparamref name="TComponent"/> as this field's input (value, gather, validation)
        /// and renders its markup inside the field wrapper. Render the markup with
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.ElementId"/> and
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.BindingPath"/>.
        /// </summary>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(IHtmlString content)
            where TComponent : IInputComponent, new()
        {
            if (content == null) throw new System.ArgumentNullException(nameof(content));
            return RenderIdentified<TComponent>(MarkupOf(content));
        }
#else
        private static string MarkupOf(IHtmlContent content)
        {
            using (var markup = new StringWriter())
            {
                content.WriteTo(markup, HtmlEncoder.Default);
                return markup.ToString();
            }
        }

        /// <summary>
        /// Registers <typeparamref name="TComponent"/> as this field's input (value, gather, validation)
        /// and renders its markup inside the field wrapper. Render the markup with
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.ElementId"/> and
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.BindingPath"/>.
        /// </summary>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(IHtmlContent content)
            where TComponent : IInputComponent, new()
        {
            if (content == null) throw new System.ArgumentNullException(nameof(content));
            return RenderIdentified<TComponent>(MarkupOf(content));
        }
#endif

        /// <summary>
        /// Registers <typeparamref name="TComponent"/> as this field's input (value, gather, validation)
        /// and renders its markup inside the field wrapper, for a vendor helper that writes its markup to
        /// a <see cref="TextWriter"/> instead of returning it.
        /// </summary>
        /// <param name="write">Writes the component markup to the writer it receives.</param>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(System.Action<TextWriter> write)
            where TComponent : IInputComponent, new()
        {
            if (write == null) throw new System.ArgumentNullException(nameof(write));
            using (var markup = new StringWriter())
            {
                write(markup);
                return RenderIdentified<TComponent>(markup.ToString());
            }
        }

        // The markup must give the field's element id to an element: the plan finds the input by it.
        private RenderedComponent<TComponent, TModel> RenderIdentified<TComponent>(string markup)
            where TComponent : IInputComponent, new()
        {
            var identified = ComponentMarkup.Identified(markup, ElementId, typeof(TComponent));
            RegisterInputComponent(InputComponentRegistrationProfile.For<TComponent>());
            Render(() => Writer.Write(identified));
            return new RenderedComponent<TComponent, TModel>(Plan, ElementId);
        }
    }
}
