using System;
using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a grid layout.
    /// </summary>
    public class GridBuilder
    {
        private readonly GridCols _cols;
        private SpacingScale _gap = SpacingScale.Md;
        private bool _responsive = true;
        private string? _cssClass;
        private string? _id;

        /// <summary>Creates a grid builder for the specified column count.</summary>
        /// <param name="cols">The number of columns to render.</param>
        public GridBuilder(GridCols cols)
        {
            _cols = cols;
        }

        /// <summary>Sets the gap token used between grid items.</summary>
        /// <param name="gap">The spacing token to apply.</param>
        /// <returns>The current builder.</returns>
        public GridBuilder Gap(SpacingScale gap)
        {
            _gap = gap;
            return this;
        }

        /// <summary>Enables or disables responsive column collapsing.</summary>
        /// <param name="responsive"><see langword="true"/> to render responsive classes; otherwise, <see langword="false"/>.</param>
        /// <returns>The current builder.</returns>
        public GridBuilder Responsive(bool responsive)
        {
            _responsive = responsive;
            return this;
        }

        /// <summary>Adds custom CSS classes to the grid element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public GridBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the grid element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public GridBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Writes the opening grid markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = _responsive
                ? GridCss.ResponsiveClasses(_cols, _gap, _cssClass)
                : GridCss.Classes(_cols, _gap, _cssClass);

            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
