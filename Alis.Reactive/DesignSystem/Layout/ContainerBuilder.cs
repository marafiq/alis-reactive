using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class ContainerBuilder
    {
        private string? _cssClass;
        private string? _id;

        public ContainerBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public ContainerBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = ContainerCss.Classes(_cssClass);
            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
