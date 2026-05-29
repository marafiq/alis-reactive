using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Native.Components
{
    public sealed class NativeActionLinkBuilder<TModel> :
        IHtmlContent
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

        public NativeActionLinkBuilder<TModel> CssClass(string css)
        {
            _cssClass = css ?? throw new ArgumentNullException(nameof(css));
            return this;
        }

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
