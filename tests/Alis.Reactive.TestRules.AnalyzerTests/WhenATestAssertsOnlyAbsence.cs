using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestAssertsOnlyAbsence
{
    private const string File = "WhenAWardIsEmptied.cs";
    private const string Rows = "_page.GetByRole(AriaRole.Row)";
    private const string Rounds = "internal static class Rounds\n{\n    internal static Task SeeTheWard(IPage page) => Expect(page.GetByRole(AriaRole.Row)).ToBeVisibleAsync();\n}\n";
    private const string Ward = "namespace Ward\n{\n    public interface ILocatorAssertions { Task ToHaveValueAsync(string value); }\n    public static class Assertions { public static ILocatorAssertions Expect(string locator) => null!; }\n    public static class Checks { public static void That(bool held) { } }\n}\n";

    [RefusedCase]
    public Task a_test_that_asserts_only_what_is_hidden_is_refused_at_its_name()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test(
                "{|#0:the_ward_shows_no_resident|}",
                "await _page.GetByRole(AriaRole.Button).ClickAsync();",
                $"await Expect({Rows}).ToBeHiddenAsync();")))
            .Expecting(OnlyAbsence(0, "the_ward_shows_no_resident"))
            .RunAsync();

    [AcceptedCase]
    public Task a_presence_beside_the_absences_is_accepted()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test(
                "the_ward_shows_no_resident",
                "await _page.GetByRole(AriaRole.Button).ClickAsync();",
                $"await Expect({Rows}).ToBeHiddenAsync();",
                "await Expect(_page.GetByRole(AriaRole.Status)).ToHaveTextAsync(\"No resident\");")))
            .RunAsync();

    [RefusedCase]
    public Task a_matcher_reached_through_not_is_an_absence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test(
                "{|#0:the_ward_shows_no_alert|}",
                "await Expect(_page.GetByRole(AriaRole.Alert)).Not.ToBeVisibleAsync();",
                "await Expect(_page).Not.ToHaveURLAsync(\"/ward\");")))
            .Expecting(OnlyAbsence(0, "the_ward_shows_no_alert"))
            .RunAsync();

    [AcceptedCase]
    public Task a_matcher_that_is_not_reached_through_not_is_a_presence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("the_ward_shows_an_alert", "await Expect(_page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();"),
                Test("the_ward_is_opened", "await Expect(_page).ToHaveURLAsync(\"/ward\");"),
                Test("the_ward_shows_its_rows", "await Shown.ToBeVisibleAsync();")))
            .RunAsync();

    [RefusedCase]
    public Task a_count_of_zero_is_an_absence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:the_ward_lists_no_resident|}", $"await Expect({Rows}).ToHaveCountAsync(0);"),
                Test("{|#1:the_ward_lists_no_visitor|}", "const int None = 0;", $"await Expect({Rows}).ToHaveCountAsync(None);")))
            .Expecting(OnlyAbsence(0, "the_ward_lists_no_resident"), OnlyAbsence(1, "the_ward_lists_no_visitor"))
            .RunAsync();

    [AcceptedCase]
    public Task a_count_above_zero_is_a_presence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test("the_ward_lists_three_residents", $"await Expect({Rows}).ToHaveCountAsync(3);")))
            .RunAsync();

    [RefusedCase]
    public Task an_empty_element_is_an_absence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test("{|#0:the_ward_shows_no_note|}", "await Expect(_page.GetByRole(AriaRole.Status)).ToBeEmptyAsync();")))
            .Expecting(OnlyAbsence(0, "the_ward_shows_no_note"))
            .RunAsync();

    [RefusedCase]
    public Task an_expected_text_or_value_that_is_empty_is_an_absence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:the_note_is_cleared|}", "await Expect(_page.GetByRole(AriaRole.Status)).ToHaveTextAsync(\"\");"),
                Test("{|#1:the_name_is_cleared|}", "const string Blank = \"\";", "await Expect(_page.GetByLabel(\"Name\")).ToHaveValueAsync(Blank);"),
                Test("{|#2:the_title_is_cleared|}", "await Expect(_page.GetByLabel(\"Name\")).ToHaveAttributeAsync(\"title\", \"\");")))
            .Expecting(OnlyAbsence(0, "the_note_is_cleared"), OnlyAbsence(1, "the_name_is_cleared"), OnlyAbsence(2, "the_title_is_cleared"))
            .RunAsync();

    [AcceptedCase]
    public Task an_expected_text_and_value_that_are_not_empty_are_presences()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("the_note_is_shown", "await Expect(_page.GetByRole(AriaRole.Status)).ToHaveTextAsync(\"Seen\");"),
                Test("the_name_is_kept", "await Expect(_page.GetByLabel(\"Name\")).ToHaveValueAsync(\"Helen\");"),
                Test("the_title_is_kept", "await Expect(_page.GetByLabel(\"Name\")).ToHaveAttributeAsync(\"title\", \"Nurse\");")))
            .RunAsync();

    [RefusedCase]
    public Task a_constraint_that_is_empty_or_null_is_an_absence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:nothing_was_sent|}", "Assert.That(_sent, Is.Null);"),
                Test("{|#1:no_name_was_sent|}", "Assert.That(_names, Is.Empty);")))
            .Expecting(OnlyAbsence(0, "nothing_was_sent"), OnlyAbsence(1, "no_name_was_sent"))
            .RunAsync();

    [AcceptedCase]
    public Task a_constraint_reached_through_not_or_given_a_value_is_a_presence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("a_request_was_sent", "Assert.That(_sent, Is.Not.Null);"),
                Test("a_name_was_sent", "Assert.That(_names, Is.Not.Empty);"),
                Test("the_name_was_sent", "Assert.That(_sent, Is.EqualTo(\"Helen\"));"),
                Test("the_gate_was_held", "Assert.That(_held, Is.True);")))
            .RunAsync();

    [AcceptedCase]
    public Task a_check_with_no_constraint_is_a_presence()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test("the_gate_was_held", "Assert.That(_held);")))
            .RunAsync();

    [RefusedCase]
    public Task an_absence_asserted_in_a_local_function_or_a_lambda_is_the_tests_own()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:the_ward_shows_no_resident|}", $"Task Hidden() => Expect({Rows}).ToBeHiddenAsync();", "await Hidden();"),
                Test("{|#1:the_ward_shows_no_note|}", "Func<Task> empty = () => Expect(_page.GetByRole(AriaRole.Status)).ToBeEmptyAsync();", "await empty();")))
            .Expecting(OnlyAbsence(0, "the_ward_shows_no_resident"), OnlyAbsence(1, "the_ward_shows_no_note"))
            .RunAsync();

    [RefusedCase]
    public Task a_test_that_asserts_nothing_is_refused_at_its_name()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:the_ward_is_opened|}", "await _page.GetByRole(AriaRole.Button).ClickAsync();", "await Rounds.SeeTheWard(_page);"),
                Test("{|#1:the_ward_is_held|}", $"var held = Expect({Rows});", "await Task.CompletedTask;")) + Rounds)
            .Expecting(NoAssertion(0, "the_ward_is_opened"), NoAssertion(1, "the_ward_is_held"))
            .RunAsync();

    [RefusedCase]
    public Task a_matcher_off_the_list_of_what_a_test_takes_is_no_assertion()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test("{|#0:the_ward_names_helen|}", $"await Expect({Rows}).ToContainTextAsync(\"Helen\");")))
            .Expecting(NoAssertion(0, "the_ward_names_helen"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_by_a_matchers_name_that_is_not_playwrights_or_nunits_is_no_assertion()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(
                Test("{|#0:the_ward_is_named|}", "await Ward.Assertions.Expect(\"ward\").ToHaveValueAsync(\"North\");"),
                Test("{|#1:the_gate_was_held|}", "Ward.Checks.That(_held);", "await Task.CompletedTask;")) + Ward)
            .Expecting(NoAssertion(0, "the_ward_is_named"), NoAssertion(1, "the_gate_was_held"))
            .RunAsync();

    [RefusedCase]
    public Task a_test_that_checks_only_null_in_a_project_with_no_playwright_is_refused()
        => new Case<TEST010TestAssertsAPresence>()
            .Without("Microsoft.Playwright.dll")
            .WithFile(
                "WhenNothingIsSent.cs",
                "public sealed class WhenNothingIsSent\n{\n    private readonly string _sent = null!;\n\n    [Test]\n    public void {|#0:nothing_was_sent|}() => Assert.That(_sent, Is.Null);\n}\n")
            .Expecting(OnlyAbsence(0, "nothing_was_sent"))
            .RunAsync();

    [AcceptedCase]
    public Task a_method_that_is_no_test_is_not_read()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(
                "Wards.cs",
                "internal static class Wards\n{\n    internal static Task Empty(IPage page) => Expect(page.GetByRole(AriaRole.Row)).ToBeHiddenAsync();\n\n    internal static Task Open(IPage page) => page.GetByRole(AriaRole.Button).ClickAsync();\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_holds_no_test()
        => new Case<TEST010TestAssertsAPresence>()
            .Without("nunit.framework.dll")
            .WithFile(
                "Wards.cs",
                "internal static class Wards\n{\n    internal static Task the_ward_shows_no_resident(IPage page) => Expect(page.GetByRole(AriaRole.Row)).ToBeHiddenAsync();\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST010TestAssertsAPresence>()
            .WithFile(File, Fixture(Test("the_ward_shows_no_resident", $"await Expect({Rows}).{{|CS1061:ToBeGoneAsync|}}();")))
            .RunAsync();

    private static string Fixture(params string[] tests)
        => "public sealed class WhenAWardIsEmptied\n{\n    private readonly IPage _page = null!;\n    private readonly string _sent = null!;\n    private readonly string[] _names = [];\n    private readonly bool _held = true;\n\n"
           + "    private ILocatorAssertions Shown => Expect(_page.GetByRole(AriaRole.Row));\n"
           + string.Concat(tests)
           + "}\n";

    private static string Test(string name, params string[] lines)
        => "\n    [Test]\n    public async Task " + name + "()\n    {\n" + string.Concat(lines.Select(line => "        " + line + "\n")) + "    }\n";

    private static DiagnosticResult OnlyAbsence(int at, string test)
        => new DiagnosticResult(TEST010TestAssertsAPresence.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{test}' asserts only what is absent; assert what the page shows when the promise holds");

    private static DiagnosticResult NoAssertion(int at, string test)
        => new DiagnosticResult(TEST010TestAssertsAPresence.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{test}' asserts nothing; assert what the page shows when the promise holds");
}
