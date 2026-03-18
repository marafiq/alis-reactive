using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.ValidationSummary;

internal static class ValidationSummaryRenderer
{
    public static void Render(TagHelperOutput output, string? planId, string? cssClass)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("data-alis-validation-summary", planId ?? "");
        output.Attributes.SetAttribute("hidden", "");

        if (!string.IsNullOrWhiteSpace(cssClass))
        {
            output.Attributes.SetAttribute("class", cssClass);
        }
    }
}
