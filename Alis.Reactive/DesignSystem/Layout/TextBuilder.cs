using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class TextBuilder
    {
        private TextSize _size = TextSize.Base;
        private TextColor _color = TextColor.Primary;
        private bool _bold;
        private bool _span;
        private string? _cssClass;
        private string? _id;

        public TextBuilder Size(TextSize size)
        {
            _size = size;
            return this;
        }

        public TextBuilder Color(TextColor color)
        {
            _color = color;
            return this;
        }

        public TextBuilder Bold(bool bold = true)
        {
            _bold = bold;
            return this;
        }

        public TextBuilder AsSpan()
        {
            _span = true;
            return this;
        }

        public TextBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public TextBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var tag = _span ? "span" : "p";
            var classes = TextCss.Classes(_size, _color, _bold, _cssClass);
            writer.Write($"<{tag}");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, $"</{tag}>");
        }
    }
}
