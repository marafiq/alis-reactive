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
        private TemplateElementId _id = TemplateElementId.None;

        /// <summary>
        /// Set the id attribute on this div
        /// </summary>
        public FusionTemplateBuilder<TModel> Id(string id)
        {
            _id = TemplateElementId.Of(id);
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
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Add a nested span element with property binding and CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Add a nested span with static text
        /// </summary>
        public FusionTemplateBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Add a nested span with static text and CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span(string text, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Add an image element bound to a property
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None, TemplateAltText.None);

        /// <summary>
        /// Add an image element bound to a property with CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.None);

        /// <summary>
        /// Add an image element bound to a property with CSS class and alt text
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css, string alt) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.Text(alt));

        private FusionTemplateBuilder<TModel> Img<TProperty>(
            Expression<Func<TModel, TProperty>> srcProperty,
            TemplateCss css,
            TemplateAltText alt)
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
        public FusionTemplateBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Add a Syncfusion icon with CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Add a button element.
        /// The <paramref name="onClick"/> value is injected directly into the onclick
        /// attribute — do not pass untrusted input.
        /// </summary>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Add a button element with CSS class.
        /// </summary>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Add a button with dynamic onClick using property value
        /// </summary>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn) =>
            ButtonFor(text, idProperty, onClickFn, TemplateCss.None);

        /// <summary>
        /// Add a button with dynamic onClick using property value and CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn,
            string css) =>
            ButtonFor(text, idProperty, onClickFn, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn,
            TemplateCss css)
        {
            _children.Add(() =>
            {
                var binding = FusionTemplateExpression.ToBinding(idProperty);
                return $"<button class=\"{css.AppendTo("e-btn")}\" onclick=\"{onClickFn}({binding})\">{text}</button>";
            });
            return this;
        }

        /// <summary>
        /// Add a button that dispatches a custom event with the row ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Add a button that dispatches a custom event with the row ID and CSS class.
        /// </summary>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty,
            string css) =>
            EventButton(text, eventName, idProperty, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty,
            TemplateCss css)
        {
            _children.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Add a link element
        /// </summary>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty) =>
            Link(hrefProperty, textProperty, TemplateCss.None);

        /// <summary>
        /// Add a link element with CSS class
        /// </summary>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty,
            string css) =>
            Link(hrefProperty, textProperty, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty,
            TemplateCss css)
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
            Action<FusionConditionalBuilder<TModel>> then) =>
            When(condition, then, TemplateElseBranch<TModel>.Missing);

        /// <summary>
        /// Conditional rendering - When condition is true, otherwise render else content
        /// </summary>
        public FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then,
            Action<FusionConditionalBuilder<TModel>> @else) =>
            When(condition, then, TemplateElseBranch<TModel>.Present(@else));

        private FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then,
            TemplateElseBranch<TModel> elseBranch)
        {
            _children.Add(() =>
            {
                var conditionStr = FusionTemplateExpression.ToCondition(condition);
                var thenBuilder = new FusionConditionalBuilder<TModel>();
                then(thenBuilder);

                var sb = new StringBuilder();
                sb.Append($"${{if({conditionStr})}}");
                sb.Append(thenBuilder.Render());

                elseBranch.AppendTo(sb);

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
            sb.Append(_id.Attribute);

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

    internal abstract class TemplateElseBranch<TModel>
    {
        private protected TemplateElseBranch() { }

        internal static TemplateElseBranch<TModel> Missing { get; } =
            new MissingTemplateElseBranch<TModel>();

        internal static TemplateElseBranch<TModel> Present(Action<FusionConditionalBuilder<TModel>> configure) =>
            new PresentTemplateElseBranch<TModel>(configure);

        internal abstract void AppendTo(StringBuilder builder);
    }

    internal sealed class MissingTemplateElseBranch<TModel> : TemplateElseBranch<TModel>
    {
        internal override void AppendTo(StringBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
        }
    }

    internal sealed class PresentTemplateElseBranch<TModel> : TemplateElseBranch<TModel>
    {
        private readonly Action<FusionConditionalBuilder<TModel>> _configure;

        internal PresentTemplateElseBranch(Action<FusionConditionalBuilder<TModel>> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        internal override void AppendTo(StringBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var elseBuilder = new FusionConditionalBuilder<TModel>();
            _configure(elseBuilder);
            builder.Append("${else}");
            builder.Append(elseBuilder.Render());
        }
    }
}
