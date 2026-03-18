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
    public static void Render(TagHelperOutput output, string label, string value, KvLayout layout)
    {
        output.TagName = null;
        var html = layout == KvLayout.Inline
            ? KvCss.InlineHtml(label, value)
            : KvCss.StackedHtml(label, value);
        output.Content.SetHtmlContent(html);
    }
}
