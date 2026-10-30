using System.Text.Encodings.Web;
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

        var layoutClass = Layout == KvLayout.Inline ? "alis-kv--inline" : "alis-kv--stacked";
        var separator = Layout == KvLayout.Inline ? ":" : "";
        output.Attributes.SetAttribute("class", LayoutClasses.Join("alis-kv", layoutClass, CssClass));
        output.Content.SetHtmlContent(
            $"<dt class=\"alis-kv__label\">{encodedLabel}{separator}</dt>" +
            $"<dd class=\"alis-kv__value\">{encodedValue}</dd>");
    }
}
