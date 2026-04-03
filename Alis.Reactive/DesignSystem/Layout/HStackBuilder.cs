using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a horizontal stack layout.
    /// </summary>
    public class HStackBuilder
    {
        private readonly SpacingScale _gap;
        private AlignItems _align = AlignItems.Center;
        private JustifyContent _justify = JustifyContent.Start;
        private bool _wrap;
        private string? _cssClass;
        private string? _id;

        /// <summary>Creates a horizontal stack builder for the specified gap token.</summary>
        /// <param name="gap">The spacing token used between items.</param>
        public HStackBuilder(SpacingScale gap)
        {
            _gap = gap;
        }

        /// <summary>Sets the cross-axis alignment for the stack.</summary>
        /// <param name="align">The alignment preset to apply.</param>
        /// <returns>The current builder.</returns>
        public HStackBuilder Align(AlignItems align)
        {
            _align = align;
            return this;
        }

        /// <summary>Sets the main-axis justification for the stack.</summary>
        /// <param name="justify">The justification preset to apply.</param>
        /// <returns>The current builder.</returns>
        public HStackBuilder Justify(JustifyContent justify)
        {
            _justify = justify;
            return this;
        }

        /// <summary>Enables or disables item wrapping.</summary>
        /// <param name="wrap"><see langword="true"/> to allow wrapping; otherwise, <see langword="false"/>.</param>
        /// <returns>The current builder.</returns>
        public HStackBuilder Wrap(bool wrap = true)
        {
            _wrap = wrap;
            return this;
        }

        /// <summary>Adds custom CSS classes to the stack element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public HStackBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the stack element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public HStackBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Writes the opening stack markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = HStackCss.Classes(_gap, _align, _justify, _wrap, _cssClass);
            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
