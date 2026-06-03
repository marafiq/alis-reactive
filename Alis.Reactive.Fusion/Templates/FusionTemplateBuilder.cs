using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Alis.Reactive.Fusion.Templates
{
    /// <summary>
    /// Entry point for typed Syncfusion template HTML builders.
    /// </summary>
    public static class FusionTemplate
    {
        /// <summary>
        /// Starts a template for rows or items shaped by <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">The model exposed by the Syncfusion template context.</typeparam>
        /// <returns>A template builder for composing the HTML string.</returns>
        public static FusionTemplateBuilder<TModel> Create<TModel>() => new FusionTemplateBuilder<TModel>();
    }

    /// <summary>
    /// Builds the root <c>div</c> for a Syncfusion template string.
    /// Use <see cref="FusionTemplate.Create{TModel}"/> to create instances.
    /// </summary>
    /// <typeparam name="TModel">The model exposed by the Syncfusion template context.</typeparam>
    public class FusionTemplateBuilder<TModel>
    {
        internal FusionTemplateBuilder() { }
        private readonly List<Func<string>> _children = new List<Func<string>>();
        private readonly List<string> _cssClasses = new List<string>();
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private TemplateElementId _id = TemplateElementId.None;

        /// <summary>
        /// Sets the <c>id</c> attribute on the root template <c>div</c>.
        /// </summary>
        /// <param name="id">The HTML element ID.</param>
        public FusionTemplateBuilder<TModel> Id(string id)
        {
            _id = TemplateElementId.Of(id);
            return this;
        }

        /// <summary>
        /// Adds a CSS class to the root template <c>div</c>.
        /// </summary>
        /// <param name="cssClass">The CSS class to append.</param>
        public FusionTemplateBuilder<TModel> Class(string cssClass)
        {
            _cssClasses.Add(cssClass);
            return this;
        }

        /// <summary>
        /// Adds a custom HTML attribute to the root template <c>div</c>.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public FusionTemplateBuilder<TModel> Attr(string name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Adds text content bound to a template model property.
        /// </summary>
        /// <param name="property">The model property rendered as text content.</param>
        public FusionTemplateBuilder<TModel> Text<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var binding = FusionTemplateExpression.ToBinding(property);
            _children.Add(() => binding);
            return this;
        }

        /// <summary>
        /// Adds static text content.
        /// </summary>
        /// <param name="text">The text rendered into the template.</param>
        public FusionTemplateBuilder<TModel> Text(string text)
        {
            _children.Add(() => text);
            return this;
        }

        /// <summary>
        /// Adds a span whose content is bound to a template model property.
        /// </summary>
        /// <param name="property">The model property rendered as span content.</param>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) =>
            Span(property, TemplateCss.None);

        /// <summary>
        /// Adds a span whose content is bound to a template model property.
        /// </summary>
        /// <param name="property">The model property rendered as span content.</param>
        /// <param name="css">The CSS class added to the span.</param>
        public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) =>
            Span(property, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a span with static text content.
        /// </summary>
        /// <param name="text">The text rendered as span content.</param>
        public FusionTemplateBuilder<TModel> Span(string text) =>
            Span(text, TemplateCss.None);

        /// <summary>
        /// Adds a span with static text content.
        /// </summary>
        /// <param name="text">The text rendered as span content.</param>
        /// <param name="css">The CSS class added to the span.</param>
        public FusionTemplateBuilder<TModel> Span(string text, string css) =>
            Span(text, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Span(string text, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Span(text, css));
            return this;
        }

        /// <summary>
        /// Adds an image whose source is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">The model property rendered as the image source.</param>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) =>
            Img(srcProperty, TemplateCss.None, TemplateAltText.None);

        /// <summary>
        /// Adds an image whose source is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">The model property rendered as the image source.</param>
        /// <param name="css">The CSS class added to the image.</param>
        public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) =>
            Img(srcProperty, TemplateCss.Class(css), TemplateAltText.None);

        /// <summary>
        /// Adds an image whose source is bound to a template model property.
        /// </summary>
        /// <param name="srcProperty">The model property rendered as the image source.</param>
        /// <param name="css">The CSS class added to the image.</param>
        /// <param name="alt">The image alt text.</param>
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
        /// Adds nested div content using another template builder.
        /// </summary>
        /// <param name="configure">Configures the nested template content.</param>
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
        /// <param name="property">The model property rendered as badge content.</param>
        /// <param name="css">The CSS class added to the badge.</param>
        public FusionTemplateBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(FusionTemplateExpression.ToBinding(property), css));
            return this;
        }

        /// <summary>
        /// Adds a badge with static text content.
        /// </summary>
        /// <param name="text">The text rendered as badge content.</param>
        /// <param name="css">The CSS class added to the badge.</param>
        public FusionTemplateBuilder<TModel> Badge(string text, string css = "e-badge")
        {
            _children.Add(() => TemplateElements.Badge(text, css));
            return this;
        }

        /// <summary>
        /// Adds a Syncfusion icon span.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        public FusionTemplateBuilder<TModel> Icon(string iconName) =>
            Icon(iconName, TemplateCss.None);

        /// <summary>
        /// Adds a Syncfusion icon span.
        /// </summary>
        /// <param name="iconName">The Syncfusion icon class name.</param>
        /// <param name="css">The additional CSS class added to the icon span.</param>
        public FusionTemplateBuilder<TModel> Icon(string iconName, string css) =>
            Icon(iconName, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Icon(string iconName, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Icon(iconName, css));
            return this;
        }

        /// <summary>
        /// Adds a button element.
        /// The <paramref name="onClick"/> value is injected directly into the onclick
        /// attribute — do not pass untrusted input.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">The JavaScript onclick expression emitted into the template.</param>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick) =>
            Button(text, onClick, TemplateCss.None);

        /// <summary>
        /// Adds a button element with a CSS class.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">The JavaScript onclick expression emitted into the template.</param>
        /// <param name="css">The CSS class added to the button.</param>
        public FusionTemplateBuilder<TModel> Button(string text, string onClick, string css) =>
            Button(text, onClick, TemplateCss.Class(css));

        private FusionTemplateBuilder<TModel> Button(string text, string onClick, TemplateCss css)
        {
            _children.Add(() => TemplateElements.Button(text, onClick, css));
            return this;
        }

        /// <summary>
        /// Adds a button whose onclick call receives a template model property value.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="idProperty">The model property passed to the onclick function.</param>
        /// <param name="onClickFn">The JavaScript function name invoked from the onclick attribute.</param>
        public FusionTemplateBuilder<TModel> ButtonFor<TProperty>(
            string text,
            Expression<Func<TModel, TProperty>> idProperty,
            string onClickFn) =>
            ButtonFor(text, idProperty, onClickFn, TemplateCss.None);

        /// <summary>
        /// Adds a button whose onclick call receives a template model property value.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="idProperty">The model property passed to the onclick function.</param>
        /// <param name="onClickFn">The JavaScript function name invoked from the onclick attribute.</param>
        /// <param name="css">The CSS class added to the button.</param>
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
        /// Adds a button that dispatches a custom event with the row ID.
        /// Uses <c>&amp;quot;</c> for event name quoting to survive SF template engine
        /// single-to-double quote conversion.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="eventName">The custom event name to dispatch.</param>
        /// <param name="idProperty">The model property passed as the event detail.</param>
        public FusionTemplateBuilder<TModel> EventButton<TProperty>(
            string text,
            string eventName,
            Expression<Func<TModel, TProperty>> idProperty) =>
            EventButton(text, eventName, idProperty, TemplateCss.None);

        /// <summary>
        /// Adds a button that dispatches a custom event with the row ID and a CSS class.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="eventName">The custom event name to dispatch.</param>
        /// <param name="idProperty">The model property passed as the event detail.</param>
        /// <param name="css">The CSS class added to the button.</param>
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
        /// Adds a link whose href and text are bound to template model properties.
        /// </summary>
        /// <param name="hrefProperty">The model property rendered as the link href.</param>
        /// <param name="textProperty">The model property rendered as the link text.</param>
        public FusionTemplateBuilder<TModel> Link<THref, TText>(
            Expression<Func<TModel, THref>> hrefProperty,
            Expression<Func<TModel, TText>> textProperty) =>
            Link(hrefProperty, textProperty, TemplateCss.None);

        /// <summary>
        /// Adds a link whose href and text are bound to template model properties.
        /// </summary>
        /// <param name="hrefProperty">The model property rendered as the link href.</param>
        /// <param name="textProperty">The model property rendered as the link text.</param>
        /// <param name="css">The CSS class added to the link.</param>
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
        /// Adds conditional content rendered when the Syncfusion template condition is true.
        /// </summary>
        /// <param name="condition">The model condition rendered into the Syncfusion template expression.</param>
        /// <param name="then">Configures the content rendered when the condition is true.</param>
        public FusionTemplateBuilder<TModel> When(
            Expression<Func<TModel, bool>> condition,
            Action<FusionConditionalBuilder<TModel>> then) =>
            When(condition, then, TemplateElseBranch<TModel>.Missing);

        /// <summary>
        /// Adds conditional content with an else branch.
        /// </summary>
        /// <param name="condition">The model condition rendered into the Syncfusion template expression.</param>
        /// <param name="then">Configures the content rendered when the condition is true.</param>
        /// <param name="else">Configures the content rendered when the condition is false.</param>
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
        /// Adds content rendered only when the Syncfusion template condition is true.
        /// </summary>
        /// <param name="condition">The model condition rendered into the Syncfusion template expression.</param>
        /// <param name="content">Configures the conditional content.</param>
        public FusionTemplateBuilder<TModel> ShowIf(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> content)
        {
            return When(condition, content);
        }

        /// <summary>
        /// Adds raw HTML content. The <paramref name="html"/> value is emitted
        /// without escaping — do not pass untrusted input.
        /// </summary>
        /// <param name="html">The raw HTML emitted into the template.</param>
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
