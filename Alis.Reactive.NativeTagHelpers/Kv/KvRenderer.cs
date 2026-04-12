using Alis.Reactive.DesignSystem.Layout;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Kv;

public enum KvLayout
{
    Stacked,
    Inline
}

internal static class KvRenderer
{
    public static void Render(TagHelperOutput output, string label, string value, KvLayout layout, string? cssClass)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (layout == KvLayout.Inline)
        {
            output.Attributes.SetAttribute("class", KvCss.InlineWrapperClasses(cssClass));
            output.Content.SetHtmlContent(
                $"<dt class=\"{KvCss.InlineDtClasses()}\">{label}:</dt>" +
                $"<dd class=\"{KvCss.InlineDdClasses()}\">{value}</dd>");
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
                output.Attributes.SetAttribute("class", cssClass);
            output.Content.SetHtmlContent(
                $"<dt class=\"{KvCss.StackedDtClasses()}\">{label}</dt>" +
                $"<dd class=\"{KvCss.StackedDdClasses()}\">{value}</dd>");
        }
    }
}
