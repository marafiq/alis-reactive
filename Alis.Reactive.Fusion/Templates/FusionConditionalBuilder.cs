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
    /// Caller-provided literal text, CSS classes, raw <c>onclick</c> values,
    /// event names, and raw HTML are emitted as supplied; use trusted developer-authored values.
    /// </remarks>
    /// <typeparam name="TModel">The object shape exposed by the Syncfusion template context.</typeparam>
    public class FusionConditionalBuilder<TModel>
    {
        private readonly List<Func<string>> _childRenderers = new List<Func<string>>();

        /// <summary>
        /// Adds a <c>span</c> bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="property">The template model property to bind.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="property">The template model property to bind.</param>
        /// <param name="css">The CSS class to emit on the <c>span</c>.</param>
        /// <returns>The current conditional builder.</returns>
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
        /// <param name="text">The literal text to emit inside the <c>span</c>.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> with literal text.
        /// </summary>
        /// <param name="text">The literal text to emit inside the <c>span</c>.</param>
        /// <param name="css">The CSS class to emit on the <c>span</c>.</param>
        /// <returns>The current conditional builder.</returns>
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
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="property">The template model property to bind.</param>
        /// <param name="css">The CSS class to emit on the badge.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with literal text.
        /// </summary>
        /// <param name="text">The literal text to emit inside the badge.</param>
        /// <param name="css">The CSS class to emit on the badge.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a styled Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        /// <param name="css">The CSS class to append to the icon <c>span</c>.</param>
        /// <returns>The current conditional builder.</returns>
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
        /// <param name="configure">Configures the nested template content.</param>
        /// <returns>The current conditional builder.</returns>
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
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="srcProperty">The template model property used for <c>src</c>.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="srcProperty">The template model property used for <c>src</c>.</param>
        /// <param name="css">The CSS class to emit on the <c>img</c>.</param>
        /// <returns>The current conditional builder.</returns>
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
        /// Adds a button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <param name="text">The literal button text.</param>
        /// <param name="onClick">The raw <c>onclick</c> expression to emit.</param>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a styled button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <param name="text">The literal button text.</param>
        /// <param name="onClick">The raw <c>onclick</c> expression to emit.</param>
        /// <param name="css">The CSS class to emit on the button.</param>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="text">The literal button text.</param>
        /// <param name="eventName">The DOM event name to dispatch.</param>
        /// <param name="idProperty">The template model property emitted as <c>detail.id</c>.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        /// <typeparam name="TProperty">The selected property type.</typeparam>
        /// <param name="text">The literal button text.</param>
        /// <param name="eventName">The DOM event name to dispatch.</param>
        /// <param name="idProperty">The template model property emitted as <c>detail.id</c>.</param>
        /// <param name="css">The CSS class to emit on the button.</param>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty,
            string css) =>
            EventButton(text, eventName, idProperty, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty,
            TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Adds raw HTML to the template output.
        /// </summary>
        /// <param name="html">The raw HTML to emit.</param>
        /// <remarks>The <paramref name="html"/> value is emitted without escaping; do not pass untrusted input.</remarks>
        /// <returns>The current conditional builder.</returns>
        public FusionConditionalBuilder<TModel> Raw(string html)
        {
            _childRenderers.Add(() => html);
            return this;
        }

        /// <summary>
        /// Renders the conditional content to an HTML string.
        /// </summary>
        /// <returns>The generated template HTML.</returns>
        public string Render()
        {
            var html = new StringBuilder();
            foreach (var renderChild in _childRenderers)
                html.Append(renderChild());
            return html.ToString();
        }
    }
}
