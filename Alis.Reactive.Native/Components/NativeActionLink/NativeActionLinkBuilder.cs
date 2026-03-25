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
#if NET48
    public sealed class NativeActionLinkBuilder<TModel> : IHtmlString
#else
    public sealed class NativeActionLinkBuilder<TModel> : IHtmlContent
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
            _elementId = elementId;
            _text = text;
            _href = href;
            _payloadJson = payloadJson;
        }

        public NativeActionLinkBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
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

            _attributes[name] = value;
            return this;
        }

#if NET48
        public string ToHtmlString()
        {
            using (var sw = new StringWriter())
            {
                WriteTo(sw, HtmlEncoder.Default);
                return sw.ToString();
            }
        }
#endif

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

            if (!string.IsNullOrWhiteSpace(_cssClass))
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
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
    }
}
