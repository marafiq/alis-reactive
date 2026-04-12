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
    public static void Render(TagHelperOutput output, DividerStyle style, string? label, string? cssClass)
    {
        if (!string.IsNullOrEmpty(label))
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", DividerCss.LabeledWrapperClasses(cssClass));
            output.Content.SetHtmlContent(
                $"<div class=\"{DividerCss.LabeledLineOuterClasses()}\">" +
                $"<div class=\"{DividerCss.LabeledLineInnerClasses()}\"></div></div>" +
                $"<div class=\"{DividerCss.LabeledTextWrapperClasses()}\">" +
                $"<span class=\"{DividerCss.LabeledTextClasses()}\">{label}</span></div>");
        }
        else
        {
            output.TagName = "hr";
            var classes = style == DividerStyle.Dashed
                ? DividerCss.DashedClasses(cssClass)
                : DividerCss.PlainClasses(cssClass);
            output.Attributes.SetAttribute("class", classes);
            output.TagMode = TagMode.SelfClosing;
        }
    }
}
