using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Creates typed Syncfusion template HTML builders.
    /// </summary>
    public static class FusionTemplate
    {
        /// <summary>
        /// Starts a template for rows, cards, list items, or other Syncfusion template contexts.
        /// </summary>
        /// <typeparam name="TModel">The object shape exposed by the Syncfusion template context.</typeparam>
        public static FusionTemplateBuilder<TModel> Create<TModel>() => new FusionTemplateBuilder<TModel>();
    }

    /// <summary>
    /// Builds a Syncfusion template HTML string with typed property bindings.
    /// </summary>
    /// <remarks>
    /// The rendered string is consumed by Syncfusion's template engine. It is not a
    /// Reactive Plan, and it does not mutate live DOM until Syncfusion renders it.
    /// Caller-provided literal text, attributes, CSS classes, URLs, inline JavaScript,
    /// and raw HTML are emitted as supplied; use trusted developer-authored values.
    /// </remarks>
    /// <typeparam name="TModel">The object shape exposed by the Syncfusion template context.</typeparam>
    public class FusionTemplateBuilder<TModel>
    {
        internal FusionTemplateBuilder() { }
        private readonly List<Func<string>> _childRenderers = new List<Func<string>>();
        private readonly List<string> _cssClasses = new List<string>();
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private TemplateElementId _id = TemplateElementId.None;

        /// <summary>
        /// Sets the root <c>div</c> ID emitted into the template string.
        /// </summary>
        /// <param name="id">Root template element ID.</param>
        public FusionTemplateBuilder<TModel> Id(string id)
        {
            _id = TemplateElementId.Of(id);
            return this;
        }

        /// <summary>
        /// Adds a CSS class to the root template <c>div</c>.
        /// </summary>
        /// <param name="cssClass">CSS class.</param>
        public FusionTemplateBuilder<TModel> Class(string cssClass)
        {
            _cssClasses.Add(cssClass);
            return this;
        }

        /// <summary>
        /// Adds or replaces an HTML attribute on the root template <c>div</c>.
        /// </summary>
        /// <param name="name">HTML attribute name.</param>
        /// <param name="value">HTML attribute value.</param>
        /// <remarks>Attribute names and values are emitted as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Attr(string name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion binding for a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        public FusionTemplateBuilder<TModel> Text<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var binding = FusionTemplateExpression.ToBinding(property);
            _childRenderers.Add(() => binding);
            return this;
        }

        /// <summary>
        /// Adds literal text to the template output.
        /// </summary>
        /// <param name="text">Literal template text.</param>
        public FusionTemplateBuilder<TModel> Text(string text)
        {
            _childRenderers.Add(() => text);
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> with literal text.
        /// </summary>
        /// <param name="text">Literal span text.</param>
        public FusionTemplateBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> with literal text.
        /// </summary>
        /// <param name="text">Literal span text.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span(string text, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Adds an <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">Template model property used for <c>src</c>.</param>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None, TemplateAltText.None);

        /// <summary>
        /// Adds a styled <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">Template model property used for <c>src</c>.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.None);

        /// <summary>
        /// Adds a styled <c>img</c> with bound <c>src</c> and static alt text.
        /// </summary>
        /// <param name="srcProperty">Template model property used for <c>src</c>.</param>
        /// <param name="css">CSS class.</param>
        /// <param name="alt">Static alt text.</param>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css, string alt) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.Text(alt));

        private FusionTemplateBuilder<TModel> Img<TProperty>(
            Expression<Func<TModel, TProperty>> srcProperty,
            TemplateCss css,
            TemplateAltText alt)
        {
            _childRenderers.Add(() => TemplateElements.Img(FusionTemplateExpression.ToBinding(srcProperty), css, alt));
            return this;
        }

        /// <summary>
        /// Adds a nested template <c>div</c> over the same Syncfusion model context.
        /// </summary>
        /// <param name="configure">Nested template content.</param>
        public FusionTemplateBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nestedTemplate = new FusionTemplateBuilder<TModel>();
            configure(nestedTemplate);
            _childRenderers.Add(() => nestedTemplate.Render());
            return this;
        }

        /// <summary>
        /// Adds a badge whose content is bound to a template model property.
        /// </summary>
        /// <param name="property">Template model property.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with literal text.
        /// </summary>
        /// <param name="text">Literal badge text.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _childRenderers.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">Syncfusion icon CSS class.</param>
        public FusionTemplateBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a styled Syncfusion icon <c>span</c>.
        /// </summary>
        /// <param name="iconName">Syncfusion icon CSS class.</param>
        /// <param name="css">CSS class.</param>
        public FusionTemplateBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds a button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="onClick">Raw <c>onclick</c> expression.</param>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a styled button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="onClick">Raw <c>onclick</c> expression.</param>
        /// <param name="css">CSS class.</param>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _childRenderers.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button that calls a JavaScript function with a bound model value.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="idProperty">Template model property passed as the JavaScript function argument.</param>
        /// <param name="onClickFn">JavaScript function name written to <c>onclick</c>.</param>
        /// <remarks>The function name is emitted into <c>onclick</c> as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn) =>
            ButtonFor(text, idProperty, onClickFn, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that calls a JavaScript function with a bound model value.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="idProperty">Template model property passed as the JavaScript function argument.</param>
        /// <param name="onClickFn">JavaScript function name written to <c>onclick</c>.</param>
        /// <param name="css">CSS class.</param>
        /// <remarks>The function name is emitted into <c>onclick</c> as supplied; do not pass untrusted input.</remarks>
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
            _childRenderers.Add(() =>
            {
                var idBinding = FusionTemplateExpression.ToBinding(idProperty);
                return $"<button class=\"{css.AppendTo("e-btn")}\" onclick=\"{onClickFn}({idBinding})\">{text}</button>";
            });
            return this;
        }

        /// <summary>
        /// Adds a button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="eventName">DOM <c>CustomEvent</c> name.</param>
        /// <param name="idProperty">Template model property emitted as <c>detail.id</c>.</param>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="eventName">DOM <c>CustomEvent</c> name.</param>
        /// <param name="idProperty">Template model property emitted as <c>detail.id</c>.</param>
        /// <param name="css">CSS class.</param>
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
            _childRenderers.Add(() => TemplateElements.EventButton(text, eventName, FusionTemplateExpression.ToBinding(idProperty), css));
            return this;
        }

        /// <summary>
        /// Adds a link whose <c>href</c> and text are bound to template model properties.
        /// </summary>
        /// <param name="hrefProperty">Template model property used for <c>href</c>.</param>
        /// <param name="textProperty">Template model property used for link text.</param>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty) =>
            Link(hrefProperty, textProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled link whose <c>href</c> and text are bound to template model properties.
        /// </summary>
        /// <param name="hrefProperty">Template model property used for <c>href</c>.</param>
        /// <param name="textProperty">Template model property used for link text.</param>
        /// <param name="css">CSS class.</param>
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
            _childRenderers.Add(() => TemplateElements.Link(
                FusionTemplateExpression.ToBinding(hrefProperty),
                FusionTemplateExpression.ToBinding(textProperty),
                css));
            return this;
        }

        /// <summary>
        /// Emits a Syncfusion <c>${if(...)}...${/if}</c> block for a typed condition.
        /// </summary>
        /// <param name="condition">Typed template condition.</param>
        /// <param name="then">Configures content emitted when the condition is true.</param>
        public FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then) =>
            When(condition, then, TemplateElseBranch<TModel>.Missing);

        /// <summary>
        /// Emits a Syncfusion <c>${if(...)}...${else}...${/if}</c> block.
        /// </summary>
        /// <param name="condition">Typed template condition.</param>
        /// <param name="then">Configures content emitted when the condition is true.</param>
        /// <param name="else">Configures content emitted when the condition is false.</param>
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
            _childRenderers.Add(() =>
            {
                var templateCondition = FusionTemplateExpression.ToCondition(condition);
                var thenBuilder = new FusionConditionalBuilder<TModel>();
                then(thenBuilder);

                var html = new StringBuilder();
                html.Append($"${{if({templateCondition})}}");
                html.Append(thenBuilder.Render());

                elseBranch.AppendTo(html);

                html.Append("${/if}");
                return html.ToString();
            });
            return this;
        }

        /// <summary>
        /// Adds conditional content when the typed condition is true.
        /// </summary>
        /// <param name="condition">Typed template condition.</param>
        /// <param name="content">Configures content emitted when the condition is true.</param>
        public FusionTemplateBuilder<TModel> ShowIf(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> content)
        {
            return When(condition, content);
        }

        /// <summary>
        /// Adds raw HTML to the template output.
        /// </summary>
        /// <param name="html">Raw HTML.</param>
        /// <remarks>The <paramref name="html"/> value is emitted without escaping; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Raw(string html)
        {
            _childRenderers.Add(() => html);
            return this;
        }

        /// <summary>
        /// Renders the root <c>div</c> and all nested content to an HTML string.
        /// </summary>
        public string Render()
        {
            var html = new StringBuilder();
            html.Append("<div");
            html.Append(_id.Attribute);

            if (_cssClasses.Count > 0)
                html.Append($" class=\"{string.Join(" ", _cssClasses)}\"");

            foreach (var attribute in _attributes)
                html.Append($" {attribute.Key}=\"{attribute.Value}\"");

            html.Append(">");

            foreach (var childRenderer in _childRenderers)
                html.Append(childRenderer());

            html.Append("</div>");
            return html.ToString();
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
