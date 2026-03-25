using System;
using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class GridBuilder
    {
        private readonly GridCols _cols;
        private SpacingScale _gap = SpacingScale.Md;
        private bool _responsive = true;
        private string? _cssClass;
        private string? _id;

        public GridBuilder(GridCols cols)
        {
            _cols = cols;
        }

        public GridBuilder Gap(SpacingScale gap)
        {
            _gap = gap;
            return this;
        }

        public GridBuilder Responsive(bool responsive)
        {
            _responsive = responsive;
            return this;
        }

        public GridBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public GridBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = _responsive
                ? GridCss.ResponsiveClasses(_cols, _gap, _cssClass)
                : GridCss.Classes(_cols, _gap, _cssClass);

            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
