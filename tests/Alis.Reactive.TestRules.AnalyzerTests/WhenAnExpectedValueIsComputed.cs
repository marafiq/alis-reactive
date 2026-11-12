using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenAnExpectedValueIsComputed
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    private readonly IPage _page = null!;\n    private readonly IRequest _request = null!;\n\n    [Test]\n    public async Task the_round_is_listed()\n    {\n";
    private const string Closing = "\n    }\n}\n";
    private const string Rounds = "internal static class Rounds\n{\n    internal static string Next() => \"Rounds\";\n    internal static int Count() => 3;\n    internal static bool IsExact() => true;\n    internal static AriaRole Role() => AriaRole.Row;\n    internal static string[] Names() => [\"Ada\", \"Ben\"];\n    internal static string Describe(string sent) => sent;\n}\n";

    [RefusedCase]
    public Task a_computed_text_given_to_a_matcher_is_refused_at_the_value()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var expected = Rounds.Next();\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync({|#0:expected|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Textbox)).ToHaveValueAsync({|#1:Rounds.Next()|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveCountAsync({|#2:Rounds.Count()|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveAttributeAsync(\"data-round\", {|#3:expected + \"!\"|});\n"
                + "        await Expect(_page).ToHaveURLAsync({|#4:$\"/rounds/{expected}\"|});"
                + Closing
                + Rounds)
            .Expecting(
                Computed(0, "expected", "ToHaveTextAsync"),
                Computed(1, "Rounds.Next()", "ToHaveValueAsync"),
                Computed(2, "Rounds.Count()", "ToHaveCountAsync"),
                Computed(3, "expected + \"!\"", "ToHaveAttributeAsync"),
                Computed(4, "$\"/rounds/{expected}\"", "ToHaveURLAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task a_literal_a_constant_and_a_generated_promise_constant_are_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        const string Heading = \"Rounds\";\n"
                + "        const int Beds = 3;\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(Heading);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync($\"{Heading} of {nameof(Rounds)}\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(Promises.Harness.WhenTwoWorldsRun_each_world_lists_only_the_residents_it_seeded);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveCountAsync(Beds + 1);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveAttributeAsync(\"data-round\", \"1\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new() { Checked = false });\n"
                + "        await Expect(_page).ToHaveURLAsync(\"/rounds\");"
                + Closing
                + Rounds)
            .RunAsync();

    [RefusedCase]
    public Task a_computed_role_name_label_text_selector_and_place_are_refused_each_at_its_value()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var name = Rounds.Next();\n"
                + "        bool? exact = Rounds.IsExact();\n"
                + "        _ = _page.GetByRole({|#0:Rounds.Role()|}, new() { Name = {|#1:name|}, Exact = {|#2:exact|}, Level = {|#3:Rounds.Count()|} });\n"
                + "        _ = _page.GetByLabel({|#4:name|});\n"
                + "        _ = _page.GetByText({|#5:name|}, new PageGetByTextOptions { Exact = {|#6:Rounds.IsExact()|} });\n"
                + "        _ = _page.Locator({|#7:\"#\" + name|});\n"
                + "        _ = _page.FrameLocator({|#8:name|}).GetByLabel({|#9:name|});\n"
                + "        _ = _page.GetByRole(AriaRole.Row).Nth({|#10:Rounds.Count()|}).Filter(new() { HasText = {|#11:name|} });"
                + Closing
                + Rounds)
            .Expecting(
                Computed(0, "Rounds.Role()", "GetByRole"),
                Computed(1, "name", "Name"),
                Computed(2, "exact", "Exact"),
                Computed(3, "Rounds.Count()", "Level"),
                Computed(4, "name", "GetByLabel"),
                Computed(5, "name", "GetByText"),
                Computed(6, "Rounds.IsExact()", "Exact"),
                Computed(7, "\"#\" + name", "Locator"),
                Computed(8, "name", "FrameLocator"),
                Computed(9, "name", "GetByLabel"),
                Computed(10, "Rounds.Count()", "Nth"),
                Computed(11, "name", "HasText"))
            .RunAsync();

    [AcceptedCase]
    public Task constants_that_find_an_element_and_values_of_another_type_are_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var pattern = new Regex(Rounds.Next());\n"
                + "        var patterns = new System.Collections.Generic.List<Regex> { pattern };\n"
                + "        Regex[] either = [pattern];\n"
                + "        var start = _page.GetByRole(AriaRole.Button, new() { Name = \"Start\", Exact = true, Level = 1 });\n"
                + "        var rows = _page.GetByRole(AriaRole.Row).Filter(new() { Has = start, HasText = \"Ada\" });\n"
                + "        _ = _page.GetByLabel(\"Round\").And(start).Or(rows).Nth(1);\n"
                + "        _ = _page.GetByText(pattern);\n"
                + "        _ = _page.Locator(\"#round\").Locator(start);\n"
                + "        _ = _page.FrameLocator(\"#answer\").GetByText(\"Ada\");\n"
                + "        var options = new PageGetByRoleOptions();\n"
                + "        options.Name = Rounds.Next();\n"
                + "        _ = _page.GetByRole(AriaRole.Heading, options);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(pattern);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(patterns);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(either);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { pattern });"
                + Closing
                + Rounds)
            .RunAsync();

    [RefusedCase]
    public Task a_computed_item_of_a_list_and_a_list_that_is_no_list_of_constants_are_refused()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var second = Rounds.Next();\n"
                + "        var names = Rounds.Names();\n"
                + "        System.Collections.Generic.IEnumerable<string> listed = names;\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { \"Ada\", {|#0:second|} });\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([\"Ada\", {|#1:second|}]);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([\"Ada\", .. {|#2:names|}]);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listbox)).ToHaveValuesAsync({|#3:names|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listbox)).ToHaveValuesAsync({|#4:new string[2]|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listbox)).ToHaveValuesAsync({|#5:new System.Collections.Generic.List<string> { \"Ada\" }|});\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listbox)).ToHaveValuesAsync({|#6:listed|});"
                + Closing
                + Rounds)
            .Expecting(
                Computed(0, "second", "ToHaveTextAsync"),
                Computed(1, "second", "ToHaveTextAsync"),
                Computed(2, "names", "ToHaveTextAsync"),
                Computed(3, "names", "ToHaveValuesAsync"),
                Computed(4, "new string[2]", "ToHaveValuesAsync"),
                Computed(5, "new System.Collections.Generic.List<string> { \"Ada\" }", "ToHaveValuesAsync"),
                Computed(6, "listed", "ToHaveValuesAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task an_array_and_a_collection_expression_of_constants_are_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        const string First = \"Ada\";\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { First, \"Ben\" });\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([First, \"Ben\"]);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new string[] { });\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveClassAsync(new[] { \"resident\", \"resident\" });\n"
                + "        await Expect(_page.GetByRole(AriaRole.Listbox)).ToHaveValuesAsync([\"Ada\"]);"
                + Closing)
            .RunAsync();

    [RefusedCase]
    public Task a_computed_value_inside_the_constraint_of_an_assertion_is_refused_at_the_value()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var expected = Rounds.Next();\n"
                + "        Assert.That(_request.PostData, Is.EqualTo({|#0:expected|}));\n"
                + "        Assert.That(_request.PostData, Does.Contain({|#1:Rounds.Next()|}).IgnoreCase);\n"
                + "        Assert.That(_request.Headers.Count, Is.EqualTo(3).Within({|#2:Rounds.Count()|}));\n"
                + "        Assert.That(_request.Headers.Keys, Is.EqualTo(new[] { \"accept\", {|#3:expected|} }));\n"
                + "        Assert.That(_request.Method, Is.AnyOf(\"GET\", {|#4:expected|}));\n"
                + "        Assert.That(_request.Url, Is.EqualTo({|#5:new Uri({|#6:expected|}).Host|}));"
                + Closing
                + Rounds)
            .Expecting(
                Computed(0, "expected", "EqualTo"),
                Computed(1, "Rounds.Next()", "Contain"),
                Computed(2, "Rounds.Count()", "Within"),
                Computed(3, "expected", "EqualTo"),
                Computed(4, "expected", "AnyOf"),
                Computed(5, "new Uri(expected).Host", "EqualTo"),
                Computed(6, "expected", "Uri"))
            .RunAsync();

    [AcceptedCase]
    public Task constants_inside_a_constraint_a_computed_actual_and_a_computed_message_are_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var pattern = new Regex(Rounds.Next());\n"
                + "        var sent = Rounds.Next();\n"
                + "        Assert.That(sent, Is.EqualTo(\"Rounds\"));\n"
                + "        Assert.That(_request.Headers.Count, Is.EqualTo(3).Within(1));\n"
                + "        Assert.That(_request.Headers.Keys, Is.EqualTo(new[] { \"accept\", \"cookie\" }));\n"
                + "        Assert.That(_request.Method, Is.AnyOf(\"GET\", \"HEAD\"));\n"
                + "        Assert.That(_request.Url, Does.Match(pattern));\n"
                + "        Assert.That(_request.PostData, Is.Not.Empty);\n"
                + "        Assert.That(sent.Length > 2, Rounds.Describe(sent));"
                + Closing
                + Rounds)
            .RunAsync();

    [RefusedCase]
    public Task a_computed_number_of_any_kind_inside_a_constraint_is_refused()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        Assert.That(_request.Headers.Count, Is.EqualTo({|#0:Numbers.Tiny()|}).Or.EqualTo({|#1:Numbers.Octet()|}).Or.EqualTo({|#2:Numbers.Short()|}).Or.EqualTo({|#3:Numbers.Port()|}));\n"
                + "        Assert.That(_request.Headers.Count, Is.EqualTo({|#4:Numbers.Count()|}).Or.EqualTo({|#5:Numbers.Tally()|}).Or.EqualTo({|#6:Numbers.Long()|}).Or.EqualTo({|#7:Numbers.Total()|}));\n"
                + "        Assert.That(_request.Headers.Count, Is.EqualTo({|#8:Numbers.Ratio()|}).Or.EqualTo({|#9:Numbers.Share()|}).Or.EqualTo({|#10:Numbers.Dose()|}));"
                + Closing
                + "internal static class Numbers\n{\n    internal static sbyte Tiny() => 1;\n    internal static byte Octet() => 1;\n    internal static short Short() => 1;\n    internal static ushort Port() => 1;\n"
                + "    internal static int Count() => 1;\n    internal static uint Tally() => 1;\n    internal static long Long() => 1;\n    internal static ulong Total() => 1;\n"
                + "    internal static float Ratio() => 1;\n    internal static double Share() => 1;\n    internal static decimal Dose() => 1;\n}\n")
            .Expecting(
                Computed(0, "Numbers.Tiny()", "EqualTo"), Computed(1, "Numbers.Octet()", "EqualTo"), Computed(2, "Numbers.Short()", "EqualTo"), Computed(3, "Numbers.Port()", "EqualTo"),
                Computed(4, "Numbers.Count()", "EqualTo"), Computed(5, "Numbers.Tally()", "EqualTo"), Computed(6, "Numbers.Long()", "EqualTo"), Computed(7, "Numbers.Total()", "EqualTo"),
                Computed(8, "Numbers.Ratio()", "EqualTo"), Computed(9, "Numbers.Share()", "EqualTo"), Computed(10, "Numbers.Dose()", "EqualTo"))
            .RunAsync();

    [RefusedCase]
    public Task a_computed_name_in_a_helper_and_in_the_harness_is_refused_too()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile("Rows.cs", "internal static class Rows { internal static ILocator Named(IPage page, string name) => page.GetByRole(AriaRole.Row, new() { Name = {|#0:name|} }); }")
            .WithFile("Harness/Frames.cs", "internal static class Frames { internal static IFrameLocator Of(IPage page, string id) => page.FrameLocator({|#1:id|}); }")
            .Expecting(Computed(0, "name", "Name"), Computed(1, "id", "FrameLocator"))
            .RunAsync();

    [AcceptedCase]
    public Task a_computed_value_given_to_a_gesture_and_to_a_member_of_another_type_is_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var name = Rounds.Next();\n"
                + "        await _page.GotoAsync(\"/rounds/\" + name);\n"
                + "        await _page.GetByLabel(\"Nurse\").FillAsync(name);\n"
                + "        Ward.ILocator ward = null!;\n"
                + "        ward.GetByLabel(name);\n"
                + "        Ward.Checks.That(name, Is.EqualTo(name));\n"
                + "        await Assert.ThatAsync(() => Task.FromResult(name), Is.EqualTo(name));"
                + Closing
                + Rounds
                + "namespace Ward\n{\n    public interface ILocator { void GetByLabel(string label); }\n    public static class Checks { public static void That(string actual, NUnit.Framework.Constraints.IResolveConstraint constraint) { } }\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_computed_value_in_a_project_with_no_nunit_is_refused()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .Without("nunit.framework.dll")
            .WithFile("Rows.cs", "internal static class Rows { internal static ILocator Named(IPage page, string name) => page.GetByLabel({|#0:name|}); }")
            .Expecting(Computed(0, "name", "GetByLabel"))
            .RunAsync();

    [RefusedCase]
    public Task a_computed_constraint_in_a_project_with_no_playwright_is_refused()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Rows.cs", "internal static class Rows { internal static void Check(string sent, string expected) => Assert.That(sent, Is.EqualTo({|#0:expected|})); }")
            .Expecting(Computed(0, "expected", "EqualTo"))
            .RunAsync();

    [AcceptedCase]
    public Task constants_in_a_project_with_no_nunit_are_accepted()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .Without("nunit.framework.dll")
            .WithFile("Rows.cs", "internal static class Rows { internal static ILocator Named(IPage page) => page.GetByLabel(\"Nurse\"); }")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST007ExpectedValueIsAConstant>()
            .WithFile("WhenARoundStarts.cs", Opening + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync({|CS0103:missing|});" + Closing)
            .RunAsync();

    private static DiagnosticResult Computed(int at, string value, string taker)
        => new DiagnosticResult(TEST007ExpectedValueIsAConstant.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{value}' is given to '{taker}' and is no constant; write the literal the promise states");
}
