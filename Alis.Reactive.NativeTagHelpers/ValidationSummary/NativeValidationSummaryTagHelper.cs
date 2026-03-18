using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.ValidationSummary;

[HtmlTargetElement("native-validation-summary")]
public class NativeValidationSummaryTagHelper : TagHelper
{
    [HtmlAttributeName("plan-id")]
    public string? PlanId { get; set; }

    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ValidationSummaryRenderer.Render(output, PlanId, CssClass);
    }
}
