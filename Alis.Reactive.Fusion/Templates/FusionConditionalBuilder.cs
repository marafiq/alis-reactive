using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Builds conditional content inside Syncfusion template <c>When</c> and <c>ShowIf</c> blocks.
    /// </summary>
    /// <typeparam name="TModel">The template model type.</typeparam>
    public class FusionConditionalBuilder<TModel>
    {
        private readonly List<Func<string>> _children = new List<Func<string>>();

        /// <summary>
        /// Adds a span whose content is bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The bound property type.</typeparam>
        /// <param name="property">The model property rendered as span content.</param>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a span whose content is bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The bound property type.</typeparam>
        /// <param name="property">The model property rendered as span content.</param>
        /// <param name="css">The CSS class added to the span.</param>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a span with static text content.
        /// </summary>
        /// <param name="text">The text rendered as span content.</param>
        public FusionConditionalBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a span with static text content.
        /// </summary>
        /// <param name="text">The text rendered as span content.</param>
        /// <param name="css">The CSS class added to the span.</param>
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
        /// <typeparam name="TProperty">The bound property type.</typeparam>
        /// <param name="property">The model property rendered as badge content.</param>
        /// <param name="css">The CSS class added to the badge.</param>
        public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with static text content.
        /// </summary>
        /// <param name="text">The text rendered as badge content.</param>
        /// <param name="css">The CSS class added to the badge.</param>
        public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon span.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        public FusionConditionalBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a Syncfusion icon span.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        /// <param name="css">The additional CSS class added to the icon span.</param>
        public FusionConditionalBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds nested div content using a template builder.
        /// </summary>
        /// <param name="configure">Configures the nested template content.</param>
        public FusionConditionalBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nested = new FusionTemplateBuilder<TModel>();
            configure(nested);
            _children.Add(() => nested.Render());
            return this;
        }

        /// <summary>
        /// Adds an image whose source is bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The bound image source property type.</typeparam>
        /// <param name="srcProperty">The model property rendered as the image source.</param>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None);

        /// <summary>
        /// Adds an image whose source is bound to a template model property.
        /// </summary>
        /// <typeparam name="TProperty">The bound image source property type.</typeparam>
        /// <param name="srcProperty">The model property rendered as the image source.</param>
        /// <param name="css">The CSS class added to the image.</param>
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
        /// Adds a button element.
        /// The <paramref name="onClick"/> value is injected directly into the onclick
        /// attribute — do not pass untrusted input.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">The JavaScript onclick expression emitted into the template.</param>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a button element with a CSS class.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">The JavaScript onclick expression emitted into the template.</param>
        /// <param name="css">The CSS class added to the button.</param>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionConditionalBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button that dispatches a custom event with the row ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        /// <typeparam name="TProperty">The row ID property type.</typeparam>
        /// <param name="text">The button text.</param>
        /// <param name="eventName">The custom event name to dispatch.</param>
        /// <param name="idProperty">The model property passed as the event detail.</param>
        public FusionConditionalBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a button that dispatches a custom event with the row ID and a CSS class.
        /// </summary>
        /// <typeparam name="TProperty">The row ID property type.</typeparam>
        /// <param name="text">The button text.</param>
        /// <param name="eventName">The custom event name to dispatch.</param>
        /// <param name="idProperty">The model property passed as the event detail.</param>
        /// <param name="css">The CSS class added to the button.</param>
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
        /// Adds raw HTML content. The <paramref name="html"/> value is emitted
        /// without escaping — do not pass untrusted input.
        /// </summary>
        /// <param name="html">The raw HTML emitted into the template.</param>
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
