using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Builds the content emitted inside a Syncfusion template conditional block.
    /// </summary>
    /// <remarks>
    /// Caller-provided literal text, CSS classes and raw HTML are
    /// emitted as supplied; use trusted developer-authored values.
    /// </remarks>
    /// <typeparam name="TModel">Syncfusion template context shape.</typeparam>
    public class FusionConditionalBuilder<TModel>
    {
        private readonly List<Func<string>> _childRenderers = new List<Func<string>>();

        /// <summary>
        /// Adds a <c>span</c> bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> with literal text.
        /// </summary>
        /// <param name="text">Literal span text.</param>
        public FusionConditionalBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> with literal text.
        /// </summary>
        /// <param name="text">Literal span text.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Span(string text, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Adds a badge whose content is bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with literal text.
        /// </summary>
        /// <param name="text">Literal badge text.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">Syncfusion icon CSS class.</param>
        public FusionConditionalBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a styled Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">Syncfusion icon CSS class.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds a nested template <c>div</c> over the same Syncfusion model context.
        /// </summary>
        /// <param name="configure">Nested template content.</param>
        public FusionConditionalBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nestedTemplate = new FusionTemplateBuilder<TModel>();
            configure(nestedTemplate);
            _childRenderers.Add(() => nestedTemplate.Render());
            return this;
        }

        /// <summary>
        /// Adds an <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">Template model property used for <c>src</c>.</param>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">Template model property used for <c>src</c>.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Img(
                FusionTemplateExpression.ToBinding(srcProperty),
                css,
                TemplateAltText.None));
            return this;
        }

        /// <summary>
        /// Adds a button whose click raises a declared document event, its payload member filled from
        /// the row: <c>.EventButton("Review", CareEvents.ReviewResident, e =&gt; e.Id, m =&gt; m.ResidentId)</c>.
        /// </summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <typeparam name="TProperty">The member's type, the same on the payload and the row.</typeparam>
        /// <param name="text">Button text.</param>
        /// <param name="raises">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="member">The payload member the button fills (a direct member).</param>
        /// <param name="from">The template model property the member's value comes from.</param>
        public FusionConditionalBuilder<TModel> EventButton<TPayload, TProperty>(
            string text,
            AppEvent<TPayload> raises,
            Expression<Func<TPayload, TProperty>> member,
            Expression<Func<TModel, TProperty>> from)
            where TPayload : class, new() =>
            EventButton(text, raises, member, from, TemplateCss.None);

        /// <summary>
        /// Adds a styled button whose click raises a declared document event, its payload member filled
        /// from the row.
        /// </summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <typeparam name="TProperty">The member's type, the same on the payload and the row.</typeparam>
        /// <param name="text">Button text.</param>
        /// <param name="raises">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="member">The payload member the button fills (a direct member).</param>
        /// <param name="from">The template model property the member's value comes from.</param>
        /// <param name="css">CSS class.</param>
        public FusionConditionalBuilder<TModel> EventButton<TPayload, TProperty>(
            string text,
            AppEvent<TPayload> raises,
            Expression<Func<TPayload, TProperty>> member,
            Expression<Func<TModel, TProperty>> from,
            string css)
            where TPayload : class, new() =>
            EventButton(text, raises, member, from, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> EventButton<TPayload, TProperty>(
            string text,
            AppEvent<TPayload> raises,
            Expression<Func<TPayload, TProperty>> member,
            Expression<Func<TModel, TProperty>> from,
            TemplateCss css)
            where TPayload : class, new()
        {
            if (raises == null) throw new ArgumentNullException(nameof(raises));
            var detailMember = TemplateElements.DetailMember(member);
            var binding = FusionTemplateExpression.ToBinding(from);
            _childRenderers.Add(() => TemplateElements.EventButton(text, raises.Name, detailMember, binding, css));
            return this;
        }

        /// <summary>
        /// Adds raw HTML to the template output.
        /// </summary>
        /// <param name="html">Raw HTML.</param>
        /// <remarks>The <paramref name="html"/> value is emitted without escaping; do not pass untrusted input.</remarks>
        public FusionConditionalBuilder<TModel> Raw(string html)
        {
            _childRenderers.Add(() => html);
            return this;
        }

        /// <summary>
        /// Renders the conditional content to an HTML string.
        /// </summary>
        /// <returns>Template HTML.</returns>
        public string Render()
        {
            var html = new StringBuilder();
            foreach (var renderChild in _childRenderers)
                html.Append(renderChild());
            return html.ToString();
        }
    }
}
