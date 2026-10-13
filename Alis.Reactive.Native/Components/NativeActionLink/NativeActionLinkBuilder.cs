using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Builds the anchor element returned by <c>NativeActionLink</c>.
    /// </summary>
    /// <typeparam name="TModel">The view's model.</typeparam>
    public sealed class NativeActionLinkBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
#if NET48
        private readonly HtmlHelper<TModel> _html;
#else
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly ReactivePlan<TModel> _plan;
        private readonly string _elementId;
        private readonly string _text;
        private readonly string _href;
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string? _cssClass;

        internal NativeActionLinkBuilder(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            string text,
            string href)
        {
            _html = html;
            _plan = plan;
            _elementId = elementId;
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _href = href ?? throw new ArgumentNullException(nameof(href));
        }

        /// <summary>
        /// Replaces the anchor's <c>class</c> attribute.
        /// </summary>
        /// <param name="css">Anchor CSS class string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="css"/> is <c>null</c>.</exception>
        public NativeActionLinkBuilder<TModel> CssClass(string css)
        {
            _cssClass = css ?? throw new ArgumentNullException(nameof(css));
            return this;
        }

        /// <summary>
        /// Adds or replaces a non-reserved HTML attribute on the anchor.
        /// </summary>
        /// <remarks>
        /// Use <c>Attr("class", value)</c> or <see cref="CssClass"/> for CSS classes.
        /// The generated <c>id</c> and <c>href</c> are reserved: the link's plan finds the anchor by
        /// its id, and its request goes to its href.
        /// </remarks>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value encoded into the rendered anchor.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is blank.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="name"/> is reserved by NativeActionLink.</exception>
        public NativeActionLinkBuilder<TModel> Attr(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

            if (string.Equals(name, "class", StringComparison.OrdinalIgnoreCase))
            {
                return CssClass(value);
            }

            if (IsReservedAttribute(name))
                throw new InvalidOperationException(
                    $"Attribute '{name}' is reserved by NativeActionLink and cannot be overridden.");

            _attributes[name] = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

#if NET48
        /// <inheritdoc />
        public string ToHtmlString()
        {
            var sw = new StringWriter();
            WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
#endif

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<a");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" href=\"");
            writer.Write(encoder.Encode(_href));
            writer.Write("\"");

            if (HasCssClass(out var cssClass))
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(cssClass));
                writer.Write("\"");
            }

            foreach (var attribute in _attributes)
            {
                writer.Write(" ");
                writer.Write(encoder.Encode(attribute.Key));
                writer.Write("=\"");
                writer.Write(encoder.Encode(attribute.Value));
                writer.Write("\"");
            }

            writer.Write(">");
            writer.Write(encoder.Encode(_text));
            writer.Write("</a>");

            // The link's own plan, rendered beside it.
#if NET48
            writer.Write(_html.RenderPlan(_plan).ToHtmlString());
#else
            _html.RenderPlan(_plan).WriteTo(writer, encoder);
#endif
        }

        private static bool IsReservedAttribute(string name)
        {
            return string.Equals(name, "id", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "href", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasCssClass(out string cssClass)
        {
            cssClass = _cssClass ?? string.Empty;
            return !string.IsNullOrWhiteSpace(cssClass);
        }
    }
}
