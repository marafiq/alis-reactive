using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for body text content.
    /// </summary>
    public class TextBuilder
    {
        private TextSize _size = TextSize.Base;
        private TextColor _color = TextColor.Primary;
        private bool _bold;
        private bool _span;
        private string? _cssClass;
        private string? _id;

        /// <summary>Sets the text size token.</summary>
        /// <param name="size">The text size to apply.</param>
        /// <returns>The current builder.</returns>
        public TextBuilder Size(TextSize size)
        {
            _size = size;
            return this;
        }

        /// <summary>Sets the text color token.</summary>
        /// <param name="color">The text color to apply.</param>
        /// <returns>The current builder.</returns>
        public TextBuilder Color(TextColor color)
        {
            _color = color;
            return this;
        }

        /// <summary>Enables or disables bold text styling.</summary>
        /// <param name="bold"><see langword="true"/> to render bold text; otherwise, <see langword="false"/>.</param>
        /// <returns>The current builder.</returns>
        public TextBuilder Bold(bool bold = true)
        {
            _bold = bold;
            return this;
        }

        /// <summary>Renders the text using a <c>span</c> element instead of a paragraph.</summary>
        /// <returns>The current builder.</returns>
        public TextBuilder AsSpan()
        {
            _span = true;
            return this;
        }

        /// <summary>Adds custom CSS classes to the text element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public TextBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the text element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public TextBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Writes the opening text markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var tag = _span ? "span" : "p";
            var classes = TextCss.Classes(_size, _color, _bold, _cssClass);
            writer.Write($"<{tag}");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, $"</{tag}>");
        }
    }
}
