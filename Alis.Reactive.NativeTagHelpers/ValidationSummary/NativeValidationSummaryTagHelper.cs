using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.ValidationSummary;

/// <summary>
/// Renders a <c>native-validation-summary</c> element: a hidden container that displays
/// the validation errors for a plan once they occur.
/// </summary>
[HtmlTargetElement("native-validation-summary")]
public class NativeValidationSummaryTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the id of the plan whose validation errors this summary displays.
    /// This attribute is required: the tag helper throws when it is missing or empty.
    /// </summary>
    [HtmlAttributeName("plan-id")]
    public string PlanId { get; set; } = "";

    /// <summary>
    /// Additional HTML classes merged with the validation summary classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(PlanId))
            throw new InvalidOperationException(
                "The native-validation-summary tag helper requires a plan-id attribute. " +
                "Set plan-id to the id of the plan whose validation errors this summary should display.");

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-reactive-validation-summary", PlanId);
        output.Attributes.SetAttribute("id", ToValidationSummaryId(PlanId));
        output.Attributes.SetAttribute("hidden", "");

        if (!string.IsNullOrWhiteSpace(CssClass))
            output.Attributes.SetAttribute("class", CssClass);
    }

    private static string ToValidationSummaryId(string planId) =>
        planId.Replace('.', '_').Replace('+', '_') + "_validation_summary";
}
