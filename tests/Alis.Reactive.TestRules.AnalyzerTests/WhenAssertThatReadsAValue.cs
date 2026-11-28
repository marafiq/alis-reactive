using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenAssertThatReadsAValue
{
    private const string Opening = "using System.Collections.Generic;\nusing System.Linq;\n\n"
                                   + "public sealed class WhenARoundIsSaved\n{\n"
                                   + "    private readonly IPage _page = null!;\n"
                                   + "    private readonly IRequest _request = null!;\n"
                                   + "    private readonly IConsoleMessage _message = null!;\n"
                                   + "    private readonly IDownload _download = null!;\n"
                                   + "    private readonly Collected _collected = new();\n"
                                   + "    private readonly int _count = 2;\n\n"
                                   + "    [Test]\n    public async Task the_round_is_posted()\n    {\n";

    private const string Closing = "\n        await _page.GotoAsync(_download.SuggestedFilename + _message.Text + _request.Url + _collected.Notes.Count + _count);\n    }\n";

    private const string TheHarness = "}\n\n"
                                      + "internal sealed class Collected\n{\n"
                                      + "    [BesideThePage]\n    internal IReadOnlyList<IConsoleMessage> Console => [];\n\n"
                                      + "    internal IReadOnlyList<string> Notes => [];\n\n"
                                      + "    [BesideThePage]\n    internal IReadOnlyList<string> UncaughtErrors() => [];\n\n"
                                      + "    internal IReadOnlyList<string> Residents() => [];\n\n"
                                      + "    [Gesture]\n    internal IReadOnlyList<string> Pushed() => [];\n}\n\n"
                                      + "namespace Wards\n{\n    internal sealed class IRequest\n    {\n        internal string Url => \"/rounds\";\n    }\n}\n";

    [RefusedCase]
    public Task a_local_a_field_and_a_parameter_are_refused_each_at_what_is_read()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        var posted = _request.PostData;\n"
                + "        Assert.That({|#0:posted|}, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That({|#1:_count|}, Is.EqualTo(2));\n"
                + "        Assert.That(expression: Is.EqualTo(2), actual: {|#2:_count|});\n"
                + "        Assert.That({|#4:(object)_count|}, Is.EqualTo(2));"
                + Closing
                + "\n    private static void Count(int rounds) => Assert.That({|#3:rounds|}, Is.EqualTo(2));\n"
                + TheHarness)
            .Expecting(Reads(0, "posted"), Reads(1, "_count"), Reads(2, "_count"), Reads(3, "rounds"), Reads(4, "(object)_count"))
            .RunAsync();

    [RefusedCase]
    public Task a_value_computed_from_the_page_the_request_itself_and_a_literal_are_refused()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That({|#0:await _page.GetByRole(AriaRole.Heading).InnerTextAsync()|}, Is.EqualTo(\"Rounds\"));\n"
                + "        Assert.That({|#1:await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), \"**/rounds\")|}, Is.Not.Null);\n"
                + "        Assert.That({|#2:_page.Url|}, Is.EqualTo(\"/rounds\"));\n"
                + "        Assert.That({|#3:true|}, Is.True);\n"
                + "        Assert.That({|#4:() => _request.PostData|}, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That({|#5:Shorten(_request.Url)|}, Is.EqualTo(\"/rounds\"));"
                + Closing
                + "\n    private static string Shorten(string url) => url.Trim();\n"
                + TheHarness)
            .Expecting(
                Reads(0, "await _page.GetByRole(AriaRole.Heading).InnerTextAsync()"),
                Reads(1, "await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), \"**/rounds\")"),
                Reads(2, "_page.Url"),
                Reads(3, "true"),
                Reads(4, "() => _request.PostData"),
                Reads(5, "Shorten(_request.Url)"))
            .RunAsync();

    [AcceptedCase]
    public Task a_listed_member_beside_the_page_is_accepted()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That(_request.Url, Is.EqualTo(\"/rounds\"));\n"
                + "        Assert.That(_request.Method, Is.EqualTo(\"POST\"));\n"
                + "        Assert.That(_request.PostData, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That(_request.Headers, Is.Not.Empty);\n"
                + "        Assert.That(await _request.HeaderValueAsync(\"x-round\"), Is.EqualTo(\"1\"));\n"
                + "        Assert.That(_message.Type, Is.EqualTo(\"error\"));\n"
                + "        Assert.That(_message.Text, Is.EqualTo(\"No round\"));\n"
                + "        Assert.That(_download.SuggestedFilename, Is.EqualTo(\"rounds.csv\"));\n"
                + "        Assert.That((await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), \"**/rounds\")).PostData, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That(expression: Is.EqualTo(\"{}\"), actual: _request.PostData);"
                + Closing
                + TheHarness)
            .RunAsync();

    [AcceptedCase]
    public Task a_call_or_a_member_read_whose_receiver_leads_to_a_listed_member_is_accepted()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That(_request.Headers[\"x-round\"], Is.EqualTo(\"1\"));\n"
                + "        Assert.That(_request.PostData!.Length, Is.EqualTo(2));\n"
                + "        Assert.That(_request.Url.Trim(), Is.EqualTo(\"/rounds\"));\n"
                + "        Assert.That(_request.Headers.Keys.Select(key => key.Length), Is.EqualTo(new[] { 7 }));\n"
                + "        Assert.That((object)_message.Text, Is.EqualTo(\"No round\"));\n"
                + "        Assert.That((await _request.HeaderValueAsync(\"x-round\")).Length, Is.EqualTo(1));"
                + Closing
                + TheHarness)
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_the_harness_that_carries_no_mark_and_a_member_of_another_assembly_named_like_a_listed_one_are_refused()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That({|#0:_collected.Notes|}, Is.Empty);\n"
                + "        Assert.That({|#1:_collected.Residents()|}, Is.Empty);\n"
                + "        Assert.That({|#2:_collected.Notes.Count|}, Is.EqualTo(0));\n"
                + "        Assert.That({|#3:_collected.Residents().Select(resident => resident.Length)|}, Is.Empty);\n"
                + "        Assert.That({|#4:_collected.Pushed()|}, Is.Empty);\n"
                + "        Assert.That({|#5:new Wards.IRequest().Url|}, Is.EqualTo(\"/rounds\"));"
                + Closing
                + TheHarness)
            .Expecting(
                Reads(0, "_collected.Notes"),
                Reads(1, "_collected.Residents()"),
                Reads(2, "_collected.Notes.Count"),
                Reads(3, "_collected.Residents().Select(resident => resident.Length)"),
                Reads(4, "_collected.Pushed()"),
                Reads(5, "new Wards.IRequest().Url"))
            .RunAsync();

    [AcceptedCase]
    public Task a_member_of_the_harness_that_carries_the_mark_and_what_leads_to_it_are_accepted()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That(_collected.Console, Is.Empty);\n"
                + "        Assert.That(_collected.UncaughtErrors(), Is.Empty);\n"
                + "        Assert.That(_collected.Console.Count, Is.EqualTo(1));\n"
                + "        Assert.That(_collected.Console.Select(message => message.Text), Is.EqualTo(new[] { \"No round\" }));\n"
                + "        Assert.That(_collected.UncaughtErrors()[0], Is.EqualTo(\"ReferenceError\"));\n"
                + "        Assert.That(_collected.UncaughtErrors().Select(error => error.Length), Is.EqualTo(new[] { 14 }));"
                + Closing
                + TheHarness)
            .RunAsync();

    [AcceptedCase]
    public Task a_that_of_another_type_and_another_member_of_nunits_assert_are_not_read()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Check.That(_count);\n"
                + "        Assert.Fail(\"No round\");"
                + Closing
                + TheHarness
                + "\ninternal static class Check { internal static void That(int count) { } }\n")
            .RunAsync();

    [RefusedCase]
    public Task a_local_is_refused_in_a_project_with_no_playwright()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("WhenARoundIsCounted.cs", "public sealed class WhenARoundIsCounted { [Test] public void the_count_is_two() { var count = 2; Assert.That({|#0:count|}, Is.EqualTo(2)); } }")
            .Expecting(Reads(0, "count"))
            .RunAsync();

    [RefusedCase]
    public Task a_local_is_refused_and_a_listed_member_accepted_in_a_project_with_no_world_contract()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .Without("Alis.Reactive.Conformance.World.dll")
            .WithFile(
                "WhenARoundIsSaved.cs",
                "public sealed class WhenARoundIsSaved\n{\n    private readonly IRequest _request = null!;\n\n"
                + "    [Test]\n    public void the_round_is_posted()\n    {\n"
                + "        var posted = _request.PostData;\n"
                + "        Assert.That(_request.PostData, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That({|#0:posted|}, Is.EqualTo(\"{}\"));\n    }\n}\n")
            .Expecting(Reads(0, "posted"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_is_not_read()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .Without("nunit.framework.dll")
            .WithFile("Rounds.cs", "internal static class Assert { internal static void That(int count) { } }\n\ninternal static class Rounds { internal static void Count(int count) => Assert.That(count); }\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST006AssertThatReadsBesideThePage>()
            .WithFile("WhenARoundIsSaved.cs", Opening + "        Assert.That({|CS0103:missing|}, Is.EqualTo(2));" + Closing + TheHarness)
            .RunAsync();

    private static DiagnosticResult Reads(int at, string read)
        => new DiagnosticResult(TEST006AssertThatReadsBesideThePage.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"Assert.That reads '{read}', which the browser does not report beside the page; assert a request, a console message or a download where it is read, and the page through Expect");
}
