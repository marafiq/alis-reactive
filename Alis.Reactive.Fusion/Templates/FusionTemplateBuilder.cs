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
        /// <returns>A builder that renders the template HTML string.</returns>
        public static FusionTemplateBuilder<TModel> Create<TModel>() => new FusionTemplateBuilder<TModel>();
    }

    /// <summary>
    /// Builds a Syncfusion template HTML string with typed property bindings.
    /// </summary>
    /// <remarks>
    /// The rendered string is consumed by Syncfusion's template engine. It is not a
    /// Reactive Plan, and it does not mutate live DOM until Syncfusion renders it.
    /// </remarks>
    /// <typeparam name="TModel">The object shape exposed by the Syncfusion template context.</typeparam>
    public class FusionTemplateBuilder<TModel>
    {
        internal FusionTemplateBuilder() { }
        private readonly List<Func<string>> _children = new List<Func<string>>();
        private readonly List<string> _cssClasses = new List<string>();
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private TemplateElementId _id = TemplateElementId.None;

        /// <summary>
        /// Sets the root <c>div</c> ID emitted into the template string.
        /// </summary>
        public FusionTemplateBuilder<TModel> Id(string id)
        {
            _id = TemplateElementId.Of(id);
            return this;
        }

        /// <summary>
        /// Adds a CSS class to the root template <c>div</c>.
        /// </summary>
        public FusionTemplateBuilder<TModel> Class(string cssClass)
        {
            _cssClasses.Add(cssClass);
            return this;
        }

        /// <summary>
        /// Adds or replaces a raw HTML attribute on the root template <c>div</c>.
        /// </summary>
        /// <remarks>Attribute names and values are emitted as supplied.</remarks>
        public FusionTemplateBuilder<TModel> Attr(string name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion binding for a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Text<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var binding = FusionTemplateExpression.ToBinding(property);
            _children.Add(() => binding);
            return this;
        }

        /// <summary>
        /// Adds static text to the template output.
        /// </summary>
        public FusionTemplateBuilder<TModel> Text(string text)
        {
            _children.Add(() => text);
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> bound to a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> bound to a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a <c>span</c> with static text.
        /// </summary>
        public FusionTemplateBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a styled <c>span</c> with static text.
        /// </summary>
        public FusionTemplateBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span(string text, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Adds an <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None, TemplateAltText.None);

        /// <summary>
        /// Adds a styled <c>img</c> whose <c>src</c> is bound to a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.None);

        /// <summary>
        /// Adds a styled <c>img</c> with bound <c>src</c> and static alt text.
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
        /// Adds a nested template <c>div</c> over the same Syncfusion model context.
        /// </summary>
        public FusionTemplateBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> configure)
        {
            var nested = new FusionTemplateBuilder<TModel>();
            configure(nested);
            _children.Add(() => nested.Render());
            return this;
        }

        /// <summary>
        /// Adds a badge whose content is bound to a template model property.
        /// </summary>
        public FusionTemplateBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with static text.
        /// </summary>
        public FusionTemplateBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon <c>span</c>.
        /// </summary>
        public FusionTemplateBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a styled Syncfusion icon <c>span</c>.
        /// </summary>
        public FusionTemplateBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds a button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a styled button with a raw <c>onclick</c> expression.
        /// </summary>
        /// <remarks>The <paramref name="onClick"/> value is emitted as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button that calls a JavaScript function with a bound model value.
        /// </summary>
        /// <remarks>The function name is emitted into <c>onclick</c> as supplied; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn) =>
            ButtonFor(text, idProperty, onClickFn, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that calls a JavaScript function with a bound model value.
        /// </summary>
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
            _children.Add(() =>
            {
                var binding = FusionTemplateExpression.ToBinding(idProperty);
                return $"<button class=\"{css.AppendTo("e-btn")}\" onclick=\"{onClickFn}({binding})\">{text}</button>";
            });
            return this;
        }

        /// <summary>
        /// Adds a button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
        /// </summary>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled button that dispatches a DOM <c>CustomEvent</c> with <c>detail.id</c> bound from the model.
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
        /// Adds a link whose <c>href</c> and text are bound to template model properties.
        /// </summary>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty) =>
            Link(hrefProperty, textProperty, TemplateCss.None);

        /// <summary>
        /// Adds a styled link whose <c>href</c> and text are bound to template model properties.
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
        /// Emits a Syncfusion <c>${if(...)}...${/if}</c> block for a typed condition.
        /// </summary>
        public FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then) =>
            When(condition, then, TemplateElseBranch<TModel>.Missing);

        /// <summary>
        /// Emits a Syncfusion <c>${if(...)}...${else}...${/if}</c> block.
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
        /// Adds conditional content when the typed condition is true.
        /// </summary>
        public FusionTemplateBuilder<TModel> ShowIf(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> content)
        {
            return When(condition, content);
        }

        /// <summary>
        /// Adds raw HTML to the template output.
        /// </summary>
        /// <remarks>The <paramref name="html"/> value is emitted without escaping; do not pass untrusted input.</remarks>
        public FusionTemplateBuilder<TModel> Raw(string html)
        {
            _children.Add(() => html);
            return this;
        }

        /// <summary>
        /// Renders the root <c>div</c> and all nested content to an HTML string.
        /// </summary>
        /// <returns>The rendered template HTML.</returns>
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
