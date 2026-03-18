using Alis.Reactive.DesignSystem.Layout;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Divider;

public enum DividerStyle
{
    Plain,
    Dashed
}

internal static class DividerRenderer
{
    public static void Render(TagHelperOutput output, DividerStyle style, string? label)
    {
        output.TagName = null;
        if (!string.IsNullOrEmpty(label))
        {
            output.Content.SetHtmlContent(DividerCss.LabeledHtml(label));
        }
        else
        {
            output.Content.SetHtmlContent(style == DividerStyle.Dashed
                ? DividerCss.DashedHtml
                : DividerCss.PlainHtml);
        }
    }
}
