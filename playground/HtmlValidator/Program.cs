using FluentValidation;
using Ganss.Xss;

// ─── Model ───
var model = new CarePlanModel
{
    CarePlan = """
        <p>Resident requires <strong>daily wound care</strong> on left ankle.</p>
        <p>Allergies: <em>Penicillin</em>, <em>Sulfa</em></p>
        <ul><li>Morning vitals at 7am</li><li>Evening check at 8pm</li></ul>
        """,
    DischargeSummary = """
        <p>Resident discharged in <strong>stable condition</strong>.</p>
        <div style="color:red;">This div should FAIL validation</div>
        <script>alert('xss')</script>
        <img src="https://evil.com/track.gif">
        """,
    Notes = """
        <p>Image from server: <img src="/uploads/photo.jpg" alt="Resident"></p>
        <p>Image from external: <img src="https://evil.com/spy.gif" alt="Spy"></p>
        <p><strong>Bold</strong> and <em>italic</em> survive.</p>
        """
};

var validator = new CarePlanValidator();
var result = validator.Validate(model);

Console.WriteLine("=== HTML Whitelist Validation ===\n");
Console.WriteLine($"Valid: {result.IsValid}\n");

if (!result.IsValid)
{
    Console.WriteLine("Failures:");
    foreach (var error in result.Errors)
        Console.WriteLine($"  [{error.PropertyName}] {error.ErrorMessage}");
}

Console.WriteLine("\n=== Default Mode (no images) ===\n");

(string html, bool expectPass)[] defaultTests =
[
    ("<p><strong>Clean</strong> content</p>",                           true),
    ("<p>With <div>div</div> inside</p>",                              false),
    ("<table><tr><td>table</td></tr></table>",                         false),
    ("<script>alert('xss')</script>",                                  false),
    ("<p>Hello</p><iframe src=\"https://evil.com\"></iframe>",         false),
    ("<p><a href=\"https://example.com\">Link</a></p>",               true),
    ("<form action=\"/steal\"><input value=\"secret\"></form>",        false),
    ("<svg onload=\"alert(1)\"><circle r=\"10\"/></svg>",              false),
    ("<p>Entity: &lt;script&gt;alert(1)&lt;/script&gt;</p>",          true),
    ("<p style=\"background:url(javascript:alert(1))\">CSS</p>",     false),
    ("<ul><li>Item 1</li><li>Item 2</li></ul>",                       true),
    ("<h1>Heading</h1><p><em>italic</em> <u>underline</u></p>",      true),
    ("<p>Text with <sub>sub</sub> and <sup>sup</sup></p>",           true),
];

RunTests(defaultTests, allowImages: false);

Console.WriteLine("\n=== Image Mode (allowImages: true) ===\n");

(string html, bool expectPass)[] imageTests =
[
    ("<p><img src=\"/images/local.jpg\" alt=\"ok\"></p>",              true),
    ("<p><img src=\"/uploads/2026/photo.png\" alt=\"upload\"></p>",    true),
    ("<p><img src=\"https://evil.com/track.gif\"></p>",               false),
    ("<p><img src=\"data:image/png;base64,abc\"></p>",                false),
    ("<p><img src=\"images/relative.jpg\"></p>",                       false),
    ("<p><img src=\"//cdn.evil.com/img.jpg\"></p>",                   false),
    ("<p><img src=\"\"></p>",                                          true),  // empty src = harmless, sanitizer strips src attr
    ("<p>Text + <img src=\"/ok.jpg\"> + <strong>bold</strong></p>",   true),
];

RunTests(imageTests, allowImages: true);

static void RunTests((string html, bool expectPass)[] tests, bool allowImages)
{
    var allCorrect = true;
    foreach (var (html, expectPass) in tests)
    {
        var actual = HtmlWhitelistValidator.IsValid(html, allowImages);
        var correct = actual == expectPass;
        if (!correct) allCorrect = false;
        var icon = correct ? (actual ? " " : "X") : "!";
        var status = actual ? "PASS" : "FAIL";
        var mismatch = correct ? "" : $" *** EXPECTED {(expectPass ? "PASS" : "FAIL")} ***";
        var display = html.Length > 55 ? html[..52] + "..." : html;
        Console.WriteLine($"  [{icon}] {status}: {display}{mismatch}");
    }
    Console.WriteLine(allCorrect ? "\n  All expectations matched." : "\n  *** SOME EXPECTATIONS DID NOT MATCH ***");
}

// ─── Validator ───
public class CarePlanValidator : AbstractValidator<CarePlanModel>
{
    public CarePlanValidator()
    {
        RuleFor(x => x.CarePlan)
            .NotEmpty()
            .HtmlWhitelist();

        RuleFor(x => x.DischargeSummary)
            .NotEmpty()
            .HtmlWhitelist();

        RuleFor(x => x.Notes)
            .HtmlWhitelist(allowImages: true); // allow <img src="/...">
    }
}

// ─── Model ───
public record CarePlanModel
{
    public string CarePlan { get; init; } = "";
    public string DischargeSummary { get; init; } = "";
    public string Notes { get; init; } = "";
}

// ─── HtmlSanitizer-based whitelist validator ───
// Uses AngleSharp (W3C-compliant parser) under the hood.
// Handles encoding tricks, entity injection, malformed HTML, mXSS — battle-tested.
public static class HtmlWhitelistValidator
{
    private static readonly string[] DefaultAllowedTags =
        ["p", "br", "strong", "em", "b", "i", "u", "ul", "ol", "li",
         "a", "h1", "h2", "h3", "h4", "h5", "h6", "span", "sub", "sup"];

    private static readonly string[] DefaultAllowedAttributes = ["href", "alt"];

    /// <summary>
    /// Validates HTML content against an allowed tag whitelist.
    /// Returns true if content contains ONLY allowed tags/attributes.
    /// </summary>
    public static bool IsValid(string html, bool allowImages = false)
    {
        if (string.IsNullOrWhiteSpace(html)) return true;

        var sanitizer = CreateSanitizer(allowImages);
        var sanitized = sanitizer.Sanitize(html);

        // If sanitizer changed anything, disallowed content was present
        return NormalizeHtml(sanitized) == NormalizeHtml(html);
    }

    /// <summary>
    /// Returns the sanitized (cleaned) version of the HTML.
    /// Useful for auto-clean on save rather than reject.
    /// </summary>
    public static string Sanitize(string html, bool allowImages = false)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;
        return CreateSanitizer(allowImages).Sanitize(html);
    }

    private static HtmlSanitizer CreateSanitizer(bool allowImages)
    {
        var sanitizer = new HtmlSanitizer();

        // Clear ALL defaults — whitelist only
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("mailto");

        // Add allowed tags
        foreach (var tag in DefaultAllowedTags)
            sanitizer.AllowedTags.Add(tag);

        // Add allowed attributes
        foreach (var attr in DefaultAllowedAttributes)
            sanitizer.AllowedAttributes.Add(attr);

        if (allowImages)
        {
            sanitizer.AllowedTags.Add("img");
            sanitizer.AllowedAttributes.Add("src");

            // Only allow images with src starting with / (not //)
            sanitizer.FilterUrl += (sender, args) =>
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

    /// <summary>
    /// Normalize whitespace/newlines for comparison — HtmlSanitizer may
    /// reformat slightly (e.g., self-closing tags, attribute order).
    /// </summary>
    private static string NormalizeHtml(string html) =>
        html.Trim()
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

    /// <summary>
    /// FluentValidation extension: .HtmlWhitelist()
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
}
