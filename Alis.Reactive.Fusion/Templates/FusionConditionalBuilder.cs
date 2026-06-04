using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Builds the content emitted inside a Syncfusion template conditional block.
    /// </summary>
    /// <typeparam name="TModel">The object shape exposed by the Syncfusion template context.</typeparam>
    public class FusionConditionalBuilder<TModel>
    {
        private readonly List<Func<string>> _children = new List<Func<string>>();

        /// <summary>
        /// Adds a <c>span</c> bound to a template model property.
        /// </summary>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> bound to a template model property.
        /// </summary>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> with static text.
        /// </summary>
        public FusionConditionalBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> with static text.
        /// </summary>
        public FusionConditionalBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Span(string text, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Adds a badge whose content is bound to a template model property.
        /// </summary>
        public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with static text.
        /// </summary>
        public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon <c>span</c>.
        /// </summary>
        public FusionConditionalBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a styled Syncfusion icon <c>span</c>.
        /// </summary>
        public FusionConditionalBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds a nested template <c>div</c> over the same Syncfusion model context.
        /// </summary>
        public FusionConditionalBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nested = new FusionTemplateBuilder<TModel>();
            configure(nested);
            _children.Add(() => nested.Render());
            return this;
        }

        /// <summary>
        /// Adds an <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Img(
                FusionTemplateExpression.ToBinding(srcProperty),
                css,
                TemplateAltText.None));
            return this;
        }

        /// <summary>
        /// Adds a button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a styled button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        public FusionConditionalBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
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
            _children.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Adds raw HTML to the template output.
        /// </summary>
        /// <remarks>The <paramref name="html"/> value is emitted without escaping; do not pass untrusted input.</remarks>
        public FusionConditionalBuilder<TModel> Raw(string html)
        {
            _children.Add(() => html);
            return this;
        }

        /// <summary>
        /// Renders the conditional content to an HTML string.
        /// </summary>
        /// <returns>The rendered HTML content.</returns>
        public string Render()
        {
            var sb = new StringBuilder();
            foreach (var child in _children)
                sb.Append(child());
            return sb.ToString();
        }
    }
}
