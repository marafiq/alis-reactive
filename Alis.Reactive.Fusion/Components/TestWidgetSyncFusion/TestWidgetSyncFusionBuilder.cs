using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Renders the standalone Syncfusion-backed test widget used by architecture verification scenarios.
    /// </summary>
    public class TestWidgetSyncFusionBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private string? _cssClass;
        private string? _initialValue;

        internal TestWidgetSyncFusionBuilder(string elementId)
        {
            _elementId = elementId;
        }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets CSS classes on the container element.</summary>
        /// <param name="css">The CSS classes to render.</param>
        /// <returns>The current builder.</returns>
        public TestWidgetSyncFusionBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Sets the initial value via the <c>data-initial-value</c> attribute.</summary>
        /// <param name="value">The initial value to render.</param>
        /// <returns>The current builder.</returns>
        public TestWidgetSyncFusionBuilder<TModel> InitialValue(string value)
        {
            _initialValue = value;
            return this;
        }

        /// <summary>
        /// Writes the widget markup.
        /// </summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <param name="encoder">The encoder used for HTML output.</param>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<div");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" data-test-widget");
            if (_initialValue != null)
            {
                writer.Write(" data-initial-value=\"");
                writer.Write(encoder.Encode(_initialValue));
                writer.Write("\"");
            }
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            writer.Write("></div>");
        }
    }
}
