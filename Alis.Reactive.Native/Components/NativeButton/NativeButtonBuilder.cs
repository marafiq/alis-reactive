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
    /// Configures and renders a native HTML <c>&lt;button&gt;</c> element with an explicit ID.
    /// </summary>
    /// <remarks>
    /// Buttons are not model-bound and do not participate in gather. The explicit
    /// element ID is the Reactive Plan event target.
    /// </remarks>
    /// <typeparam name="TModel">The view model type for the current Razor view.</typeparam>
    public class NativeButtonBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private string? _cssClass;
        private string _buttonType = "button";

        internal NativeButtonBuilder(string elementId, string text)
        {
            _elementId = elementId;
            _text = text;
        }

        internal string ElementId => _elementId;

        /// <summary>
        /// Selects the button <c>type</c> attribute.
        /// </summary>
        /// <param name="type">The HTML button type. Defaults to <c>button</c>.</param>
        public NativeButtonBuilder<TModel> Type(string type)
        {
            _buttonType = type;
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on the button element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeButtonBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
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
            writer.Write("<button");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" type=\"");
            writer.Write(encoder.Encode(_buttonType));
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
