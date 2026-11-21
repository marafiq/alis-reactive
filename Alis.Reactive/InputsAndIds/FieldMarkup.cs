using System.Net;

namespace Alis.Reactive.InputsAndIds;

internal static class FieldMarkup
{
    internal static string Open(ReactiveClassNames names, InputFieldOptions options, string inputId)
    {
        var wrapper = $"<div class=\"{WebUtility.HtmlEncode(names.Field)}\">";
        if (!options.HasLabel) return wrapper;
        var marker = options.IsRequired ? $" <span class=\"{WebUtility.HtmlEncode(names.FieldRequired)}\">*</span>" : "";
        return wrapper + $"<label class=\"{WebUtility.HtmlEncode(names.FieldLabel)}\" for=\"{WebUtility.HtmlEncode(inputId)}\">{WebUtility.HtmlEncode(options.LabelText)}{marker}</label>";
    }

    internal static string Close(ReactiveClassNames names, string messageId)
        => $"<span id=\"{WebUtility.HtmlEncode(messageId)}\" class=\"{WebUtility.HtmlEncode(names.FieldMessage)}\" hidden></span></div>";
}
