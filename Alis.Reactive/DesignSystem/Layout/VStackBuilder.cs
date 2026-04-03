using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a vertical stack layout.
    /// </summary>
    public class VStackBuilder
    {
        private readonly SpacingScale _gap;
        private bool _divideY;
        private string? _cssClass;
        private string? _id;

        /// <summary>Creates a vertical stack builder for the specified gap token.</summary>
        /// <param name="gap">The spacing token used between items.</param>
        public VStackBuilder(SpacingScale gap)
        {
            _gap = gap;
        }

        /// <summary>Enables or disables vertical dividers between items.</summary>
        /// <param name="divideY"><see langword="true"/> to render dividers; otherwise, <see langword="false"/>.</param>
        /// <returns>The current builder.</returns>
        public VStackBuilder DivideY(bool divideY = true)
        {
            _divideY = divideY;
            return this;
        }

        /// <summary>Adds custom CSS classes to the stack element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public VStackBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the stack element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public VStackBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Writes the opening stack markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = VStackCss.Classes(_gap, _divideY, _cssClass);
            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
