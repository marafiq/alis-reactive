using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenAPageIsMatchedInPart
{
    private const string Opening = "public sealed class WhenARoundIsSaved\n{\n"
                                   + "    private static readonly Regex Shown = new(\"Rounds\");\n\n"
                                   + "    private readonly IPage _page = null!;\n"
                                   + "    private readonly IRequest _request = null!;\n\n"
                                   + "    [Test]\n    public async Task the_round_is_listed()\n    {\n";

    private const string Closing = "\n        await _page.GotoAsync(_request.Url + Shown);\n    }\n}\n";

    private const string TheTestsOwn = "\ninternal sealed class Rounds\n{\n"
                                       + "    internal Regex? Pattern { get; set; }\n\n"
                                       + "    internal static void Named(Regex pattern) { }\n\n"
                                       + "    internal static void ToContainTextAsync(string text) { }\n\n"
                                       + "    internal static void Read()\n    {\n"
                                       + "        Named(new Regex(\"[0-9]+\"));\n"
                                       + "        ToContainTextAsync(\"Rounds\");\n"
                                       + "        _ = new Rounds { Pattern = new Regex(\"[0-9]+\") };\n    }\n}\n";

    [RefusedCase]
    public Task a_partial_match_of_the_text_is_refused_at_its_name()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).{|#0:ToContainTextAsync|}(\"Rounds\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).{|#1:ToContainTextAsync|}(new[] { \"Helen\", \"Marta\" });\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).Not.{|#2:ToContainTextAsync|}(\"Rounds\");"
                + Closing)
            .Expecting(
                InPart(0, "ILocatorAssertions.ToContainTextAsync"),
                InPart(1, "ILocatorAssertions.ToContainTextAsync"),
                InPart(2, "ILocatorAssertions.ToContainTextAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task the_whole_text_a_list_of_whole_texts_and_a_whole_address_are_accepted()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { \"Helen\", \"Marta\" });\n"
                + "        await Expect(_page).ToHaveURLAsync(\"/rounds\");\n"
                + "        await _page.GetByText(\"Rounds\").ClickAsync();\n"
                + "        await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), \"**/rounds\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveAttributeAsync(name: \"title\", value: \"Rounds\");"
                + Closing)
            .RunAsync();

    [RefusedCase]
    public Task a_regex_passed_to_a_member_of_playwrights_is_refused_at_the_argument()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync({|#0:new Regex(\"Rounds\")|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync({|#1:new[] { new Regex(\"Helen\"), new Regex(\"Marta\") }|});\n"
                + "        await Expect(_page).ToHaveURLAsync({|#2:new Regex(\"/rounds\")|});\n"
                + "        await _page.GetByText({|#3:Shown|}).ClickAsync();\n"
                + "        await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), {|#4:new Regex(\"/rounds\")|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveAttributeAsync(name: \"title\", value: {|#5:Shown|});"
                + Closing)
            .Expecting(
                Passed(0, "ILocatorAssertions.ToHaveTextAsync"),
                Passed(1, "ILocatorAssertions.ToHaveTextAsync"),
                Passed(2, "IPageAssertions.ToHaveURLAsync"),
                Passed(3, "IPage.GetByText"),
                Passed(4, "IPage.RunAndWaitForRequestAsync"),
                Passed(5, "ILocatorAssertions.ToHaveAttributeAsync"))
            .RunAsync();

    [RefusedCase]
    public Task a_partial_match_by_a_regex_is_refused_at_its_name_and_at_the_argument()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening + "        await Expect(_page.GetByRole(AriaRole.Heading)).{|#0:ToContainTextAsync|}({|#1:Shown|});" + Closing)
            .Expecting(InPart(0, "ILocatorAssertions.ToContainTextAsync"), Passed(1, "ILocatorAssertions.ToContainTextAsync"))
            .RunAsync();

    [RefusedCase]
    public Task a_regex_set_on_an_option_of_playwrights_is_refused_at_the_options_name()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        await _page.GetByRole(AriaRole.Button, new() { {|#0:NameRegex|} = new Regex(\"Save\") }).ClickAsync();\n"
                + "        await _page.Locator(\"#rounds\").Filter(new() { {|#1:HasTextRegex|} = Shown }).ClickAsync();\n"
                + "        var named = new PageGetByRoleOptions();\n"
                + "        named.{|#2:NameRegex|} = Shown;\n"
                + "        await _page.GetByRole(AriaRole.Button, named).ClickAsync();"
                + Closing)
            .Expecting(
                Set(0, "PageGetByRoleOptions.NameRegex"),
                Set(1, "LocatorFilterOptions.HasTextRegex"),
                Set(2, "PageGetByRoleOptions.NameRegex"))
            .RunAsync();

    [AcceptedCase]
    public Task a_text_set_on_an_option_of_playwrights_is_accepted()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        await _page.GetByRole(AriaRole.Button, new() { Name = \"Save\", Exact = true }).ClickAsync();\n"
                + "        await _page.Locator(\"#rounds\").Filter(new() { HasText = \"Helen\" }).ClickAsync();\n"
                + "        var named = new PageGetByRoleOptions();\n"
                + "        named.Name = \"Save\";\n"
                + "        await _page.GetByRole(AriaRole.Button, named).ClickAsync();"
                + Closing)
            .RunAsync();

    [AcceptedCase]
    public Task a_regex_that_reads_a_requests_header_and_a_regex_of_the_tests_own_are_accepted()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening
                + "        Assert.That(_request.Headers[\"x-round\"], Does.Match(new Regex(\"^[0-9]+$\")));\n"
                + "        Rounds.Named(Shown);\n"
                + "        Rounds.ToContainTextAsync(\"Rounds\");\n"
                + "        _ = new Rounds { Pattern = Shown };"
                + Closing
                + TheTestsOwn)
            .RunAsync();

    [RefusedCase]
    public Task a_partial_match_in_a_helper_and_in_the_harness_is_refused_too()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile("Pages.cs", "internal static class Pages { internal static Task Shows(IPage page) => Expect(page.GetByRole(AriaRole.Heading)).{|#0:ToContainTextAsync|}(\"Rounds\"); }")
            .WithFile("Harness/Found.cs", "internal static class Found { internal static ILocator Heading(IPage page) => page.GetByText({|#1:new Regex(\"Rounds\")|}); }")
            .WithFile("Harness/Named.cs", "internal static class Named { internal static PageGetByRoleOptions Save() => new() { {|#2:NameRegex|} = new Regex(\"Save\") }; }")
            .Expecting(
                InPart(0, "ILocatorAssertions.ToContainTextAsync"),
                Passed(1, "IPage.GetByText"),
                Set(2, "PageGetByRoleOptions.NameRegex"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_playwright_is_not_read()
        => new Case<TEST008PageIsMatchedWhole>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Rounds.cs", TheTestsOwn)
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST008PageIsMatchedWhole>()
            .WithFile(
                "WhenARoundIsSaved.cs",
                Opening + "        await Expect(_page.GetByRole(AriaRole.Heading)).{|CS1061:ToContainTextsAsync|}(new Regex(\"Rounds\"));" + Closing)
            .RunAsync();

    private static DiagnosticResult InPart(int at, string member)
        => Refused(at, $"'{member}' matches a part of the text; expect the whole text with ToHaveTextAsync");

    private static DiagnosticResult Passed(int at, string member)
        => Refused(at, $"'{member}' is passed a Regex, which matches a part; pass the whole text as a literal");

    private static DiagnosticResult Set(int at, string option)
        => Refused(at, $"'{option}' is set to a Regex, which matches a part; set the option that takes the whole text");

    private static DiagnosticResult Refused(int at, string message)
        => new DiagnosticResult(TEST008PageIsMatchedWhole.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
