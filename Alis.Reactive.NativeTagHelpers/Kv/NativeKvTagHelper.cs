using System.Text.Encodings.Web;
using Alis.Reactive.DesignSystem.Layout;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Kv;

/// <summary>
/// Renders a <c>native-kv</c> element as a key/value pair using a <c>dl</c> definition list.
/// </summary>
[HtmlTargetElement("native-kv")]
public class NativeKvTagHelper : TagHelper
{
    /// <summary>
    /// Required label shown for the pair. The tag helper throws when this attribute
    /// is missing or empty.
    /// </summary>
    [HtmlAttributeName("label")]
    public string Label { get; set; } = "";

    /// <summary>
    /// Required value shown for the pair. The tag helper throws when this attribute
    /// is missing or empty.
    /// </summary>
    [HtmlAttributeName("value")]
    public string Value { get; set; } = "";

    /// <summary>
    /// Controls how the label and value are arranged. Defaults to <see cref="KvLayout.Stacked"/>.
    /// </summary>
    public KvLayout Layout { get; set; } = KvLayout.Stacked;

    /// <summary>
    /// Additional HTML classes merged with the generated key/value classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Label))
            throw new InvalidOperationException(
                "The native-kv tag helper requires a non-empty label attribute. A key/value pair cannot render without a label.");

        if (string.IsNullOrWhiteSpace(Value))
            throw new InvalidOperationException(
                "The native-kv tag helper requires a non-empty value attribute. A key/value pair cannot render without a value.");

        output.TagName = "dl";
        output.TagMode = TagMode.StartTagAndEndTag;

        var encodedLabel = HtmlEncoder.Default.Encode(Label);
        var encodedValue = HtmlEncoder.Default.Encode(Value);

        if (Layout == KvLayout.Inline)
        {
            output.Attributes.SetAttribute("class", KvCss.InlineWrapperClasses(CssClass));
            output.Content.SetHtmlContent(
                $"<dt class=\"{KvCss.InlineDtClasses()}\">{encodedLabel}:</dt>" +
                $"<dd class=\"{KvCss.InlineDdClasses()}\">{encodedValue}</dd>");
            return;
        }

        var wrapperClasses = KvCss.StackedWrapperClasses(CssClass);
        if (!string.IsNullOrEmpty(wrapperClasses))
            output.Attributes.SetAttribute("class", wrapperClasses);

        output.Content.SetHtmlContent(
            $"<dt class=\"{KvCss.StackedDtClasses()}\">{encodedLabel}</dt>" +
            $"<dd class=\"{KvCss.StackedDdClasses()}\">{encodedValue}</dd>");
    }
}
