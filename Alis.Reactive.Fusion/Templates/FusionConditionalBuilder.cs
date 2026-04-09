using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Builder for conditional content inside When/ShowIf blocks
    /// </summary>
    public class FusionConditionalBuilder<TModel>
    {
        private readonly List<Func<string>> _children = new List<Func<string>>();

        /// <summary>
        /// Add a span element with property binding
        /// </summary>
        public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css = null)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Add a span with static text
        /// </summary>
        public FusionConditionalBuilder<TModel> Span(string text, string css = null)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Add a badge element with property binding
        /// </summary>
        public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Add a badge with static text
        /// </summary>
        public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Add a Syncfusion icon
        /// </summary>
        public FusionConditionalBuilder<TModel> Icon(string iconName, string css = null)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Add a nested div element
        /// </summary>
        public FusionConditionalBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nested = new FusionTemplateBuilder<TModel>();
            configure(nested);
            _children.Add(() => nested.Render());
            return this;
        }

        /// <summary>
        /// Add an image element bound to a property
        /// </summary>
        public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css = null)
        {
            _children.Add(() => TemplateElements.Img(FusionTemplateExpression.ToBinding(srcProperty), css, null));
            return this;
        }

        /// <summary>
        /// Add a button element.
        /// The <paramref name="onClick"/> value is injected directly into the onclick
        /// attribute — do not pass untrusted input.
        /// </summary>
        public FusionConditionalBuilder<TModel> Button(string text, string onClick, string css = null)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Add a button that dispatches a custom event with the row ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        public FusionConditionalBuilder<TModel> EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty, string css = null)
        {
            _children.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Add raw HTML content. The <paramref name="html"/> value is emitted
        /// without escaping — do not pass untrusted input.
        /// </summary>
        public FusionConditionalBuilder<TModel> Raw(string html)
        {
            _children.Add(() => html);
            return this;
        }

        /// <summary>
        /// Render the conditional content to an HTML string
        /// </summary>
        public string Render()
        {
            var sb = new StringBuilder();
            foreach (var child in _children)
                sb.Append(child());
            return sb.ToString();
        }
    }
}
