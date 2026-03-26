using FluentValidation;
using Ganss.Xss;

namespace YourApp.Validation;

/// <summary>
/// HTML allowlist validator using HtmlSanitizer (AngleSharp W3C parser).
/// Works on any string property via FluentValidation: RuleFor(x => x.Html).HtmlWhitelist()
///
/// NuGet: dotnet add package HtmlSanitizer
///        dotnet add package FluentValidation
/// </summary>
public static class HtmlWhitelistValidator
{
    private static readonly string[] DefaultAllowedTags =
        ["p", "br", "strong", "em", "b", "i", "u", "ul", "ol", "li",
         "a", "h1", "h2", "h3", "h4", "h5", "h6", "span", "sub", "sup"];

    private static readonly string[] DefaultAllowedAttributes = ["href", "alt"];

    /// <summary>
    /// Validates HTML against the allowlist. Returns true if ONLY allowed tags/attributes are present.
    /// </summary>
    public static bool IsValid(string? html, bool allowImages = false)
    {
        if (string.IsNullOrWhiteSpace(html)) return true;

        var sanitizer = CreateSanitizer(allowImages);
        var sanitized = sanitizer.Sanitize(html);

        return NormalizeHtml(sanitized) == NormalizeHtml(html);
    }

    /// <summary>
    /// Returns the sanitized (clean) version. Use for auto-clean on save instead of reject.
    /// </summary>
    public static string Sanitize(string? html, bool allowImages = false)
    {
        if (string.IsNullOrWhiteSpace(html)) return html ?? "";
        return CreateSanitizer(allowImages).Sanitize(html);
    }

    /// <summary>
    /// FluentValidation extension. Works on any string property.
    /// <code>
    /// RuleFor(x => x.CarePlan).HtmlWhitelist();
    /// RuleFor(x => x.Notes).HtmlWhitelist(allowImages: true);
    /// </code>
    /// </summary>
    public static IRuleBuilderOptions<T, string> HtmlWhitelist<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        bool allowImages = false)
    {
        return ruleBuilder
            .Must(html => IsValid(html, allowImages))
            .WithMessage(
                "Content contains disallowed HTML. " +
                "Only formatting tags (bold, italic, lists, headings, links) are allowed.");
    }

    private static HtmlSanitizer CreateSanitizer(bool allowImages)
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("mailto");

        foreach (var tag in DefaultAllowedTags)
            sanitizer.AllowedTags.Add(tag);

        foreach (var attr in DefaultAllowedAttributes)
            sanitizer.AllowedAttributes.Add(attr);

        if (allowImages)
        {
            sanitizer.AllowedTags.Add("img");
            sanitizer.AllowedAttributes.Add("src");

            sanitizer.FilterUrl += (_, args) =>
            {
                if (args.Tag?.LocalName != "img") return;
                var url = (args.OriginalUrl ?? "").Trim();
                var isLocalPath = url.StartsWith('/') && !url.StartsWith("//") && url.Length > 1;
                if (!isLocalPath)
                    args.SanitizedUrl = "";
            };
        }

        return sanitizer;
    }

    private static string NormalizeHtml(string html) =>
        html.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
}
