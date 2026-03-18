using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Heading;

internal static class HeadingRenderer
{
    public static void Render(TagHelperOutput output, HeadingLevel level, string? overline, string? cssClass)
    {
        output.TagName = $"h{(int)level}";
        var classes = HeadingCss.Classes(level, cssClass);
        output.Attributes.SetAttribute("class", classes);

        if (!string.IsNullOrEmpty(overline))
        {
            output.PreElement.SetHtmlContent(
                $"<p class=\"{HeadingCss.OverlineClasses()}\">{overline}</p>");
        }
    }
}
