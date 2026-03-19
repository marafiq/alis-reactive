using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.ValidationSummary;

internal static class ValidationSummaryRenderer
{
    public static void Render(TagHelperOutput output, string? planId, string? cssClass)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("data-alis-validation-summary", planId ?? "");
        output.Attributes.SetAttribute("hidden", "");

        if (!string.IsNullOrWhiteSpace(planId))
        {
            output.Attributes.SetAttribute("id", ToSummaryId(planId));
        }

        if (!string.IsNullOrWhiteSpace(cssClass))
        {
            output.Attributes.SetAttribute("class", cssClass);
        }
    }

    /// <summary>
    /// Generates a predictable HTML ID for the validation summary element.
    /// Convention: {planId with dots→underscores}_validation_summary.
    /// Matches the TS runtime's findSummaryElement() ID-based lookup.
    /// </summary>
    public static string ToSummaryId(string planId) =>
        planId.Replace('.', '_').Replace('+', '_') + "_validation_summary";
}
