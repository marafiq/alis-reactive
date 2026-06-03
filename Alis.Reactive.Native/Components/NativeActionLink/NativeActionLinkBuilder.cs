using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Builds the anchor element returned by <c>NativeActionLink</c>.
    /// </summary>
    /// <typeparam name="TModel">The view model type for the Razor view.</typeparam>
    public sealed class NativeActionLinkBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private readonly string _href;
        private readonly string _payloadJson;
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string? _cssClass;

        internal NativeActionLinkBuilder(string elementId, string text, string href, string payloadJson)
        {
            _elementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _href = href ?? throw new ArgumentNullException(nameof(href));
            _payloadJson = payloadJson ?? throw new ArgumentNullException(nameof(payloadJson));
        }

        /// <summary>
        /// Sets the anchor's <c>class</c> attribute.
        /// </summary>
        /// <param name="css">The CSS class string to render on the anchor.</param>
        /// <returns>The current builder.</returns>
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
        /// The generated <c>id</c>, <c>href</c>, and <c>data-reactive-link</c> attributes
        /// are reserved because they bind the anchor to the reactive runtime.
        /// </remarks>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value to encode and render.</param>
        /// <returns>The current builder.</returns>
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
            writer.Write(" data-reactive-link=\"");
            writer.Write(encoder.Encode(_payloadJson));
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
        }

        private static bool IsReservedAttribute(string name)
        {
            return string.Equals(name, "id", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "href", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "data-reactive-link", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasCssClass(out string cssClass)
        {
            cssClass = _cssClass ?? string.Empty;
            return !string.IsNullOrWhiteSpace(cssClass);
        }
    }
}
