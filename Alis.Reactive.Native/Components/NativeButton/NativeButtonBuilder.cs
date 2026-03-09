using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;button&gt; element with an explicit id.
    /// Not model-bound — buttons don't carry data.
    ///
    /// Usage:
    ///   @Html.NativeButton("save-btn", "Save")
    ///       .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    ///       .Reactive(plan, evt => evt.Click, (args, p) =>
    ///       {
    ///           p.Post("/api/save", g => g.Static("name", "John"))
    ///            .Response(r => r.OnSuccess(s => ...));
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public class NativeButtonBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private readonly string _text;
        private string? _cssClass;
        private string _type = "button";

        public NativeButtonBuilder(string elementId, string text)
        {
            _elementId = elementId;
            _text = text;
        }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets the button type attribute (default: "button").</summary>
        public NativeButtonBuilder<TModel> Type(string type)
        {
            _type = type;
            return this;
        }

        /// <summary>Sets CSS classes on the button element.</summary>
        public NativeButtonBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

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

    /// <summary>
    /// Factory extension for creating NativeButtonBuilder.
    /// </summary>
    public static class NativeButtonHtmlExtensions
    {
        /// <summary>
        /// Creates a native &lt;button&gt; builder with an explicit element ID.
        /// </summary>
        public static NativeButtonBuilder<TModel> NativeButton<TModel>(
            this IHtmlHelper<TModel> html, string elementId, string text)
            where TModel : class
        {
            return new NativeButtonBuilder<TModel>(elementId, text);
        }
    }
}
