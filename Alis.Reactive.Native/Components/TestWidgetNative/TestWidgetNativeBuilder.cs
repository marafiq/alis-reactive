using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders an &lt;input type="text"&gt; element for architecture verification.
    /// Not model-bound — for standalone native test inputs.
    ///
    /// Usage:
    ///   @(Html.TestWidgetNative&lt;TModel&gt;("my-input")
    ///       .InitialValue("hello")
    ///       .CssClass("rounded-md border"))
    /// </summary>
    public class TestWidgetNativeBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private string? _cssClass;
        private string? _initialValue;
        private bool _readOnly;

        internal TestWidgetNativeBuilder(string elementId)
        {
            _elementId = elementId;
        }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets CSS classes on the input element.</summary>
        public TestWidgetNativeBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Sets the initial value attribute.</summary>
        public TestWidgetNativeBuilder<TModel> InitialValue(string value)
        {
            _initialValue = value;
            return this;
        }

        /// <summary>Makes the input readonly.</summary>
        public TestWidgetNativeBuilder<TModel> ReadOnly()
        {
            _readOnly = true;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<input");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" type=\"text\"");
            if (_initialValue != null)
            {
                writer.Write(" value=\"");
                writer.Write(encoder.Encode(_initialValue));
                writer.Write("\"");
            }
            if (_readOnly)
            {
                writer.Write(" readonly");
            }
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            writer.Write(" />");
        }
    }
}
