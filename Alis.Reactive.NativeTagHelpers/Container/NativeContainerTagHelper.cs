using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Container;

[HtmlTargetElement("native-container")]
public class NativeContainerTagHelper : TagHelper
{
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ContainerRenderer.Render(output, CssClass);
    }
}
