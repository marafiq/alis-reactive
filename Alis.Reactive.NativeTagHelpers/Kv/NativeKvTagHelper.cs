using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Kv;

[HtmlTargetElement("native-kv")]
public class NativeKvTagHelper : TagHelper
{
    [HtmlAttributeName("label")]
    public string Label { get; set; } = "";
    [HtmlAttributeName("value")]
    public string Value { get; set; } = "";
    public KvLayout Layout { get; set; } = KvLayout.Stacked;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        KvRenderer.Render(output, Label, Value, Layout);
    }
}
