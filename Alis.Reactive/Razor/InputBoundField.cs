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

namespace Alis.Reactive
{
    /// <summary>
    /// Model-bound input field returned by
    /// <see cref="InputFieldExtensions.InputField{TModel,TProp}"/>, ready to receive
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

        /// <summary>
        /// Registers <typeparamref name="TComponent"/> as this field's input (value, gather, validation)
        /// and renders its markup inside the field wrapper. Render the markup with
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.ElementId"/> and
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.BindingPath"/>; markup built as text
        /// that gives no element that ID fails here, naming the component.
        /// </summary>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(IHtmlString content)
            where TComponent : IInputComponent, new()
        {
            ComponentMarkup.RequireElementId(content, ElementId, typeof(TComponent));
            RegisterInputComponent(InputComponentRegistrationProfile.For<TComponent>());
            Render(content);
            return new RenderedComponent<TComponent, TModel>(Plan, ElementId);
        }
#else
        internal void Render(IHtmlContent content)
        {
            Render(() => content.WriteTo(Writer, HtmlEncoder.Default));
        }

        /// <summary>
        /// Registers <typeparamref name="TComponent"/> as this field's input (value, gather, validation)
        /// and renders its markup inside the field wrapper. Render the markup with
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.ElementId"/> and
        /// <see cref="InputBoundFieldBase{THelper, TModel, TProp}.BindingPath"/>; markup built as text
        /// that gives no element that ID fails here, naming the component.
        /// </summary>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(IHtmlContent content)
            where TComponent : IInputComponent, new()
        {
            ComponentMarkup.RequireElementId(content, ElementId, typeof(TComponent));
            RegisterInputComponent(InputComponentRegistrationProfile.For<TComponent>());
            Render(content);
            return new RenderedComponent<TComponent, TModel>(Plan, ElementId);
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
            RegisterInputComponent(InputComponentRegistrationProfile.For<TComponent>());
            Render(() => write(Writer));
            return new RenderedComponent<TComponent, TModel>(Plan, ElementId);
        }
    }
}
