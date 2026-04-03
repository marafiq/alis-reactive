using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Builds a native button element with an explicit id for reactive wiring.
    /// </summary>
    public class NativeButtonBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private string? _cssClass;
        private string _type = "button";

        internal NativeButtonBuilder(string elementId, string text)
        {
            _elementId = elementId;
            _text = text;
        }

        /// <summary>Gets the rendered element id used for event wiring.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets the button <c>type</c> attribute.</summary>
        /// <param name="type">The button type to render.</param>
        /// <returns>The current builder.</returns>
        public NativeButtonBuilder<TModel> Type(string type)
        {
            _type = type;
            return this;
        }

        /// <summary>Adds custom CSS classes to the button element.</summary>
        /// <param name="css">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public NativeButtonBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Writes the button markup.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <param name="encoder">The encoder used for HTML attribute and content values.</param>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<button");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" type=\"");
            writer.Write(encoder.Encode(_type));
            writer.Write("\"");
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            writer.Write(">");
            writer.Write(encoder.Encode(_text));
            writer.Write("</button>");
        }
    }
}
