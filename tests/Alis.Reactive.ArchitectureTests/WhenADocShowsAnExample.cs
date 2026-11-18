using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenADocShowsAnExample
{
    private const string Readme = "README.md";
    private const string PageDoors = "spec/Reference/page-doors.json";
    private const string TheConformanceApp = "tests/Alis.Reactive.Conformance.App";
    private const string Cure = "an example is copied whole, line for line, from a view a browser test opens, as spec/Reference/page-doors.json lists it, or from a file of a fresh app";
    private const string FreshView = "tests/Alis.Reactive.Consumer.Net10/Views/Home/Index.cshtml";
    private const string OpenedView = "tests/Alis.Reactive.Conformance.App/Care/Residents/Census.cshtml";
    private const string UnopenedView = "tests/Alis.Reactive.Conformance.App/Care/Residents/Discharge.cshtml";
    private const string ASkill = ".claude/skills/census/SKILL.md";

    private const string TheFreshHomeView = """
        @using Alis.Reactive.RenderingAndDelivery
        @using Alis.Reactive.TriggersAndPipelines
        @{
            ViewData["Title"] = "Home";
            var plan = Html.ReactivePlan<dynamic>();
        }
        <p id="consumer-status">Not booted</p>
        @{
            Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("consumer-status").Text.Set("Booted")));
        }
        @Html.RenderPlan(plan)

        """;

    private const string ACensusView = """
        @model Census
        @{
            var plan = Html.ReactivePlan<Census>();
        }
        <dd id="ward-count"></dd>
        @Html.RenderPlan(plan)

        """;

    private const string APageDoorsTable = """
        {
          "M2.F1 X4.9 0": {
            "modules": [],
            "views": [
              "/Care/Residents/Census.cshtml",
              "/Shared/_Layout.cshtml"
            ]
          }
        }

        """;

    private static readonly string[] FreshApps = ["tests/Alis.Reactive.Consumer.Net10/", "tests/Alis.Reactive.Consumer.Net48/"];
    private static readonly string[] NoExample = ["bash", "text"];
    private static readonly string[] NoExampleOfASkill = ["bash", "text", "csharp"];

    [Test]
    public void every_example_the_readme_shows_is_copied_whole_from_a_page_a_browser_test_runs()
    {
        var readme = File.ReadAllText(Path.Combine(Repository.Root, Readme));

        Assert.That(ExamplesOf(readme, NoExample), Is.Not.Empty, "README.md shows no example");
        Assert.That(
            Problems(Readme, readme, NoExample, Repository.TrackedFiles, File.ReadAllText(Path.Combine(Repository.Root, PageDoors)), file => File.ReadAllText(Path.Combine(Repository.Root, file))),
            Is.Empty);
    }

    [Test]
    public void every_view_example_a_skill_shows_is_copied_whole_from_a_page_a_browser_test_runs()
    {
        var pageDoors = File.ReadAllText(Path.Combine(Repository.Root, PageDoors));
        var skills = Repository.TrackedFiles.Where(file => file.StartsWith(".claude/skills/", StringComparison.Ordinal) && file.EndsWith("/SKILL.md", StringComparison.Ordinal)).ToList();

        Assert.That(
            skills.SelectMany(skill => Problems(skill, File.ReadAllText(Path.Combine(Repository.Root, skill)), NoExampleOfASkill, Repository.TrackedFiles, pageDoors, file => File.ReadAllText(Path.Combine(Repository.Root, file)))),
            Is.Empty);
    }

    [RefusedCase]
    public void an_example_one_character_off_the_view_it_was_copied_from_is_refused()
        => Assert.That(
            Problems(
                Readme,
                "# Fresh\n\n```cshtml\n" + TheFreshHomeView.Replace("Not booted", "Not booted.", StringComparison.Ordinal) + "```\n",
                NoExample,
                [FreshView],
                APageDoorsTable,
                Files((FreshView, TheFreshHomeView))),
            Is.EqualTo(new[] { "README.md:3: its cshtml example is found whole in no view a browser test opens and no file of a fresh app; " + Cure }));

    [RefusedCase]
    public void an_example_copied_from_a_view_no_browser_test_opens_is_refused()
        => Assert.That(
            Problems(
                Readme,
                "# Census\n\nA view.\n\n```cshtml\n" + ACensusView + "```\n",
                NoExample,
                [UnopenedView],
                APageDoorsTable,
                Files((UnopenedView, ACensusView))),
            Is.EqualTo(new[] { "README.md:5: its cshtml example is found whole in no view a browser test opens and no file of a fresh app; " + Cure }));

    [AcceptedCase]
    public void an_example_copied_whole_from_a_fresh_apps_file_or_a_view_a_test_opens_and_a_command_are_accepted()
        => Assert.That(
            Problems(
                Readme,
                "# Fresh\n\n```bash\ndotnet add package AlisReactive\n```\n\n```cshtml\n" + TheFreshHomeView + "```\n\n~~~cshtml\n" + ACensusView + "~~~\n",
                NoExample,
                [FreshView, OpenedView],
                APageDoorsTable,
                Files((FreshView, TheFreshHomeView), (OpenedView, ACensusView))),
            Is.Empty);

    [RefusedCase]
    public void a_skills_view_example_one_character_off_its_view_is_refused()
        => Assert.That(
            Problems(
                ASkill,
                "# Census\n\n```cshtml\n" + ACensusView.Replace("ward-count", "ward-counts", StringComparison.Ordinal) + "```\n",
                NoExampleOfASkill,
                [OpenedView],
                APageDoorsTable,
                Files((OpenedView, ACensusView))),
            Is.EqualTo(new[] { ".claude/skills/census/SKILL.md:3: its cshtml example is found whole in no view a browser test opens and no file of a fresh app; " + Cure }));

    [AcceptedCase]
    public void a_skills_view_example_copied_whole_beside_a_declaration_it_quotes_is_accepted()
        => Assert.That(
            Problems(
                ASkill,
                "# Census\n\n```csharp\npublic int Count => 0;\n```\n\n```cshtml\n" + ACensusView + "```\n",
                NoExampleOfASkill,
                [OpenedView],
                APageDoorsTable,
                Files((OpenedView, ACensusView))),
            Is.Empty);

    private sealed record Example(int Line, string Language, IReadOnlyList<string> Lines);

    private static Func<string, string> Files(params (string Path, string Text)[] files)
        => path => files.Single(file => file.Path == path).Text;

    private static List<string> Problems(string doc, string markdown, string[] noExample, IReadOnlyList<string> tracked, string pageDoors, Func<string, string> read)
    {
        var sources = SourcesOfAnExample(tracked, pageDoors).Select(file => LinesOf(read(file))).ToList();
        return ExamplesOf(markdown, noExample)
            .Where(example => !sources.Any(source => Holds(source, example.Lines)))
            .Select(example => $"{doc}:{example.Line}: its {example.Language} example is found whole in no view a browser test opens and no file of a fresh app; {Cure}")
            .ToList();
    }

    private static IEnumerable<string> SourcesOfAnExample(IReadOnlyList<string> tracked, string pageDoors)
    {
        using var table = JsonDocument.Parse(pageDoors);
        var opened = table.RootElement.EnumerateObject()
            .SelectMany(promise => promise.Value.GetProperty("views").EnumerateArray())
            .Select(view => TheConformanceApp + view.GetString())
            .ToHashSet(StringComparer.Ordinal);
        return tracked.Where(file => opened.Contains(file) || FreshApps.Any(app => file.StartsWith(app, StringComparison.Ordinal))).ToList();
    }

    private static List<Example> ExamplesOf(string markdown, string[] noExample)
    {
        var lines = LinesOf(markdown);
        var examples = new List<Example>();
        for (var at = 0; at < lines.Count; at++)
        {
            var fence = FenceOf(lines[at]);
            if (fence is null) continue;
            var close = lines.FindIndex(at + 1, line => Closes(line, fence));
            var end = close < 0 ? lines.Count : close;
            var language = lines[at].TrimStart()[fence.Length..].Trim().Split(' ')[0];
            if (!noExample.Contains(language)) examples.Add(new Example(at + 1, language, lines[(at + 1)..end]));
            at = end;
        }

        return examples;
    }

    private static string? FenceOf(string line)
    {
        var indented = line.Length - line.TrimStart(' ').Length;
        var text = line.TrimStart(' ');
        if (indented > 3 || text.Length < 3 || (text[0] != '`' && text[0] != '~')) return null;
        var run = text.TakeWhile(character => character == text[0]).Count();
        return run >= 3 ? text[..run] : null;
    }

    private static bool Closes(string line, string fence)
    {
        var text = line.Trim();
        return text.Length >= fence.Length && text.All(character => character == fence[0]);
    }

    private static bool Holds(List<string> source, IReadOnlyList<string> example)
        => Enumerable.Range(0, Math.Max(0, source.Count - example.Count + 1)).Any(at => source.Skip(at).Take(example.Count).SequenceEqual(example, StringComparer.Ordinal));

    private static List<string> LinesOf(string text) => [.. text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')];
}
