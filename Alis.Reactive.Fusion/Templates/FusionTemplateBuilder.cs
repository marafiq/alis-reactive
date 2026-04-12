using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Factory for creating typed Syncfusion template builders.
    /// </summary>
    public static class FusionTemplate
    {
        /// <summary>
        /// Create a typed template builder for the specified model type.
        /// </summary>
        public static FusionTemplateBuilder<TModel> Create<TModel>() => new FusionTemplateBuilder<TModel>();
    }

    /// <summary>
    /// Builds a div element with nested content for Syncfusion templates.
    /// Use <see cref="FusionTemplate.Create{TModel}"/> to create instances.
    /// </summary>
    public class FusionTemplateBuilder<TModel>
    {
        internal FusionTemplateBuilder() { }
        private readonly List<Func<string>> _children = new List<Func<string>>();
        private readonly List<string> _cssClasses = new List<string>();
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private string _id;

        /// <summary>
        /// Set the id attribute on this div
        /// </summary>
        public FusionTemplateBuilder<TModel> Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Add a CSS class to this div
        /// </summary>
        public FusionTemplateBuilder<TModel> Class(string cssClass)
        {
            _cssClasses.Add(cssClass);
            return this;
        }

        /// <summary>
        /// Add a custom HTML attribute to this div
        /// </summary>
        public FusionTemplateBuilder<TModel> Attr(string name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Add text content bound to a property
        /// </summary>
        public FusionTemplateBuilder<TModel> Text<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var binding = FusionTemplateExpression.ToBinding(property);
            _children.Add(() => binding);
            return this;
        }

        /// <summary>
        /// Add static text content
        /// </summary>
        public FusionTemplateBuilder<TModel> Text(string text)
        {
            _children.Add(() => text);
            return this;
        }

        /// <summary>
        /// Add a nested span element with property binding
        /// </summary>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css = null)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Add a nested span with static text
        /// </summary>
        public FusionTemplateBuilder<TModel> Span(string text, string css = null)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Add an image element bound to a property
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css = null, string alt = null)
        {
            _children.Add(() => TemplateElements.Img(FusionTemplateExpression.ToBinding(srcProperty), css, alt));
            return this;
        }

        /// <summary>
        /// Add a nested div element
        /// </summary>
        public FusionTemplateBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nested = new FusionTemplateBuilder<TModel>();
            configure(nested);
            _children.Add(() => nested.Render());
            return this;
        }

        /// <summary>
        /// Add a badge element with property binding
        /// </summary>
        public FusionTemplateBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Add a badge with static text
        /// </summary>
        public FusionTemplateBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Add a Syncfusion icon
        /// </summary>
        public FusionTemplateBuilder<TModel> Icon(string iconName, string css = null)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Add a button element.
        /// The <paramref name="onClick"/> value is injected directly into the onclick
        /// attribute — do not pass untrusted input.
        /// </summary>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick, string css = null)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Add a button with dynamic onClick using property value
        /// </summary>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(string text, Expression<Func<TModel, TProperty>> idProperty, string onClickFn, string css = null)
        {
            _children.Add(() =>
            {
                var binding = FusionTemplateExpression.ToBinding(idProperty);
                var classAttr = string.IsNullOrEmpty(css) ? "e-btn" : $"e-btn {css}";
                return $"<button class=\"{classAttr}\" onclick=\"{onClickFn}({binding})\">{text}</button>";
            });
            return this;
        }

        /// <summary>
        /// Add a button that dispatches a custom event with the row ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(string text, string eventName, Expression<Func<TModel, TProperty>> idProperty, string css = null)
        {
            _children.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Add a link element
        /// </summary>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty,
            string css = null)
        {
            _children.Add(() => TemplateElements.Link(
                FusionTemplateExpression.ToBinding(hrefProperty),
                FusionTemplateExpression.ToBinding(textProperty),
                css));
            return this;
        }

        /// <summary>
        /// Conditional rendering - When condition is true
        /// </summary>
        public FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then,
            Action<FusionConditionalBuilder<TModel>> @else = null)
        {
            _children.Add(() =>
            {
                var conditionStr = FusionTemplateExpression.ToCondition(condition);
                var thenBuilder = new FusionConditionalBuilder<TModel>();
                then(thenBuilder);

                var sb = new StringBuilder();
                sb.Append($"${{if({conditionStr})}}");
                sb.Append(thenBuilder.Render());

                if (@else != null)
                {
                    var elseBuilder = new FusionConditionalBuilder<TModel>();
                    @else(elseBuilder);
                    sb.Append("${else}");
                    sb.Append(elseBuilder.Render());
                }

                sb.Append("${/if}");
                return sb.ToString();
            });
            return this;
        }

        /// <summary>
        /// Show content only if condition is true
        /// </summary>
        public FusionTemplateBuilder<TModel> ShowIf(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> content)
        {
            return When(condition, content);
        }

        /// <summary>
        /// Add raw HTML content. The <paramref name="html"/> value is emitted
        /// without escaping — do not pass untrusted input.
        /// </summary>
        public FusionTemplateBuilder<TModel> Raw(string html)
        {
            _children.Add(() => html);
            return this;
        }

        /// <summary>
        /// Render this div and all nested content to an HTML string
        /// </summary>
        public string Render()
        {
            var sb = new StringBuilder();
            sb.Append("<div");

            if (_id != null)
                sb.Append($" id=\"{_id}\"");

            if (_cssClasses.Count > 0)
                sb.Append($" class=\"{string.Join(" ", _cssClasses)}\"");

            foreach (var attr in _attributes)
                sb.Append($" {attr.Key}=\"{attr.Value}\"");

            sb.Append(">");

            foreach (var child in _children)
                sb.Append(child());

            sb.Append("</div>");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString() => Render();
    }
}
