using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAViewIsWritten
{
    private const string NoJavaScript = "a view carries no JavaScript: a behavior is a plan's, and a component's code is its package's";

    private static readonly Regex Script = new(@"<script\b[^>]*>(?<body>[\s\S]*?)</script\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Handler = new(@"<[a-z][^<>]*?\s(?<name>on[a-z]+)\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ScriptUrl = new(@"=\s*[""']\s*javascript:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private sealed record Written(string File, string Text);

    [Test]
    public void no_view_carries_javascript()
    {
        var views = Repository.TrackedFiles
            .Where(file => file.EndsWith(".cshtml", StringComparison.Ordinal))
            .Select(file => new Written(file, File.ReadAllText(Path.Combine(Repository.Root, file))))
            .ToList();

        Assert.That(views, Is.Not.Empty, "No tracked view was found");
        Assert.That(Problems(views), Is.Empty, "A view holds script: " + NoJavaScript + ".");
    }

    [RefusedCase]
    public void an_inline_script_is_refused_at_its_line()
        => Assert.That(
            Problems([new("Care/Round.cshtml", "<h1>Round</h1>\n<script>\n  document.title = 'Round';\n</script>\n@section Scripts {\n    <script type=\"module\">import './round.js';</script>\n}\n<SCRIPT>start()</SCRIPT>\n")]),
            Is.EqualTo(new[]
            {
                "Care/Round.cshtml:2: an inline script; " + NoJavaScript,
                "Care/Round.cshtml:6: an inline script; " + NoJavaScript,
                "Care/Round.cshtml:8: an inline script; " + NoJavaScript,
            }));

    [RefusedCase]
    public void a_handler_attribute_is_refused_at_its_line()
        => Assert.That(
            Problems([new("Care/Round.cshtml", "<h1>Round</h1>\n<button type=\"button\" class=\"start\"\n        onclick=\"start()\">Start</button>\n<BODY ONLOAD='boot()'>\n")]),
            Is.EqualTo(new[]
            {
                "Care/Round.cshtml:2: the handler attribute onclick; " + NoJavaScript,
                "Care/Round.cshtml:4: the handler attribute onload; " + NoJavaScript,
            }));

    [RefusedCase]
    public void a_script_url_is_refused_at_its_line()
        => Assert.That(
            Problems([new("Care/Round.cshtml", "<h1>Round</h1>\n<a href=\"javascript:void(0)\">Start</a>\n")]),
            Is.EqualTo(new[] { "Care/Round.cshtml:2: a javascript: URL; " + NoJavaScript }));

    [AcceptedCase]
    public void a_linked_script_and_csharp_that_only_looks_like_markup_are_accepted()
        => Assert.That(
            Problems(
            [
                new("Shared/_Layout.cshtml", "<head>\n    <script src=\"~/js/alis-reactive.js\" asp-append-version=\"true\"></script>\n    <script src=\"~/js/hub.js\">\n    </script>\n</head>\n"),
                new("Care/Round.cshtml", "@{\n    var online = Model.Nurses.Count < 4 && Model.Open;\n    List<string> onward = [];\n}\n<button type=\"button\" class=\"@(online ? \"on\" : \"off\")\" data-once=\"true\">Start</button>\n<p>The on-call nurse is on = duty.</p>\n"),
            ]),
            Is.Empty);

    private static List<string> Problems(IReadOnlyList<Written> views)
    {
        var problems = new List<string>();
        foreach (var view in views)
        {
            var found = Script.Matches(view.Text).Where(script => script.Groups["body"].Value.Trim().Length > 0)
                .Select(script => (script.Index, What: "an inline script"))
                .Concat(Handler.Matches(view.Text).Select(handler => (handler.Index, What: "the handler attribute " + handler.Groups["name"].Value.ToLowerInvariant())))
                .Concat(ScriptUrl.Matches(view.Text).Select(url => (url.Index, What: "a javascript: URL")))
                .OrderBy(refused => refused.Index);
            problems.AddRange(found.Select(refused => $"{view.File}:{LineOf(view.Text, refused.Index)}: {refused.What}; {NoJavaScript}"));
        }

        return problems;
    }

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;
}
