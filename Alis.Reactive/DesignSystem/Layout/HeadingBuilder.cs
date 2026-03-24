using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class HeadingBuilder
    {
        private HeadingLevel _level = HeadingLevel.H2;
        private string? _overline;
        private string? _cssClass;
        private string? _id;

        public HeadingBuilder Level(HeadingLevel level)
        {
            _level = level;
            return this;
        }

        public HeadingBuilder Overline(string overline)
        {
            _overline = overline;
            return this;
        }

        public HeadingBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public HeadingBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(_overline))
            {
                writer.Write($"<p class=\"{HeadingCss.OverlineClasses()}\">{_overline}</p>");
            }

            var tag = $"h{(int)_level}";
            var classes = HeadingCss.Classes(_level, _cssClass);
            writer.Write($"<{tag}");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, $"</{tag}>");
        }
    }
}
