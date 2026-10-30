using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.ValidationSummary;

/// <summary>
/// Renders a <c>native-validation-summary</c> element: the summary of a form, placed where the view writes it. It
/// holds the messages no field of the form shows (a hidden field's, a member no input renders), hidden until one
/// arrives.
/// </summary>
/// <remarks>
/// A form's summary renders once per page. Without this tag it renders after the first plan that validates the
/// form or shows its errors; with it, the plan renders none, so place the tag before that plan's
/// <c>@Html.RenderPlan(plan)</c>. A tag after it, or a second tag for the form, is refused.
/// </remarks>
/// <example><c>&lt;native-validation-summary form="room-hold-form" /&gt;</c></example>
[HtmlTargetElement("native-validation-summary")]
public class NativeValidationSummaryTagHelper(IHtmlHelper html) : TagHelper
{
    /// <summary>
    /// Required element id of the form whose summary this is: the container a request validates
    /// (<c>Validate&lt;TValidator&gt;(formId)</c>) or shows the server's errors on (<c>e.ValidationErrors(formId)</c>).
    /// </summary>
    [HtmlAttributeName("form")]
    public string Form { get; set; } = "";

    /// <summary>
    /// Caller-supplied classes written to the validation summary container.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <summary>The view the tag renders in, whose page records the form's summary.</summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Form))
            throw new InvalidOperationException(
                "The native-validation-summary tag helper requires a form attribute: the element id of the form a request " +
                "validates (Validate<TValidator>(\"formId\")) or shows the server's errors on (e.ValidationErrors(\"formId\")).");

        // The same placement as Html.FormSummary, which records the summary with the page so it renders once.
        ((IViewContextAware)html).Contextualize(ViewContext);
        output.TagName = null;
        output.Content.SetHtmlContent(html.FormSummary(Form, CssClass));
    }
}
