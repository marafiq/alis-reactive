using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for heading content.
    /// </summary>
    public class HeadingBuilder
    {
        private HeadingLevel _level = HeadingLevel.H2;
        private string? _overline;
        private string? _cssClass;
        private string? _id;

        /// <summary>Sets the heading level to render.</summary>
        /// <param name="level">The heading level to apply.</param>
        /// <returns>The current builder.</returns>
        public HeadingBuilder Level(HeadingLevel level)
        {
            _level = level;
            return this;
        }

        /// <summary>Sets the optional overline text rendered above the heading.</summary>
        /// <param name="overline">The overline text.</param>
        /// <returns>The current builder.</returns>
        public HeadingBuilder Overline(string overline)
        {
            _overline = overline;
            return this;
        }

        /// <summary>Adds custom CSS classes to the heading element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public HeadingBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the heading element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public HeadingBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Writes the opening heading markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(_overline))
            {
                writer.Write($"<p class='{HeadingCss.OverlineClasses()}'>{_overline}</p>");
            }

            var tag = $"h{(int)_level}";
            var classes = HeadingCss.Classes(_level, _cssClass);
            writer.Write($"<{tag}");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, $"</{tag}>");
        }
    }
}
