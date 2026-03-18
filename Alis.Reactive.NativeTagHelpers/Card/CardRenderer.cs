using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

internal static class CardRenderer
{
    public static void RenderOpen(TagHelperOutput output, CardElevation elevation, AccentColor? accent, string? cssClass)
    {
        output.TagName = "div";
        var classes = CardCss.CardClasses(elevation);
        if (accent.HasValue)
            classes = CssUtils.MergeClasses(classes, CardCss.AccentInnerClasses(accent.Value));
        classes = CssUtils.MergeClasses(classes, cssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
