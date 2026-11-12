using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenAHelperAsserts
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    private readonly IPage _page = null!;\n\n";

    [RefusedCase]
    public Task an_expect_in_a_helper_is_refused_at_its_name()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test] public Task the_round_is_listed() => Listed(_page.GetByRole(AriaRole.Listitem));\n\n"
                + "    private static Task Listed(ILocator rows) => {|#0:Expect|}(rows).ToHaveCountAsync(2);\n\n"
                + "    private static Task Titled(IPage page) => Assertions.{|#1:Expect|}(page).ToHaveURLAsync(\"/rounds\");\n}\n")
            .Expecting(Asserts(0, "Listed", "Expect"), Asserts(1, "Titled", "Expect"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_assert_in_a_helper_is_refused_at_its_name()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test] public void the_round_is_listed() => Counted(4);\n\n"
                + "    private static void Counted(int beds) => Assert.{|#0:That|}(beds, Is.EqualTo(4));\n\n"
                + "    private static void Never() => Assert.{|#1:Fail|}(\"never\");\n}\n")
            .Expecting(Asserts(0, "Counted", "Assert.That"), Asserts(1, "Never", "Assert.Fail"))
            .RunAsync();

    [AcceptedCase]
    public Task an_assertion_in_a_test_its_lambda_or_its_local_function_is_accepted()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test]\n    public async Task the_round_is_listed()\n    {\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n"
                + "        Assert.That(4, Is.EqualTo(4));\n"
                + "        Func<Task> titled = () => Expect(_page).ToHaveURLAsync(\"/rounds\");\n"
                + "        Task Listed() => Expect(_page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(2);\n"
                + "        await titled();\n"
                + "        await Listed();\n    }\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task an_assertion_in_a_lambda_a_local_function_a_constructor_a_property_or_a_field_of_no_test_is_refused()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "Rounds.cs",
                "public sealed class Rounds\n{\n"
                + "    private readonly Task _listed = {|#0:Expect|}((ILocator)null!).ToHaveCountAsync(2);\n\n"
                + "    public Rounds(IPage page) => _ = {|#1:Expect|}(page).ToHaveURLAsync(\"/rounds\");\n\n"
                + "    static Rounds() => _ = {|#5:Expect|}((IPage)null!).ToHaveURLAsync(\"/rounds\");\n\n"
                + "    public Task Titled => {|#2:Expect|}((IPage)null!).ToHaveURLAsync(\"/rounds\");\n\n"
                + "    public Task Open(IPage page)\n    {\n"
                + "        Func<Task> titled = () => {|#3:Expect|}(page).ToHaveURLAsync(\"/rounds\");\n"
                + "        Task Listed() => {|#4:Expect|}(page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(2);\n"
                + "        return Task.WhenAll(titled(), Listed(), _listed);\n    }\n}\n")
            .Expecting(Asserts(0, "_listed", "Expect"), Asserts(1, "Rounds", "Expect"), Asserts(2, "Titled", "Expect"), Asserts(3, "Open", "Expect"), Asserts(4, "Open", "Expect"),
                Asserts(5, "Rounds", "Expect"))
            .RunAsync();

    [RefusedCase]
    public Task an_assertion_in_the_harness_is_refused_too()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "Harness/ConformanceFixture.cs",
                "public abstract class ConformanceFixture\n{\n    [TearDown] public void EndTheWorld() => Assert.{|#0:That|}(0, Is.EqualTo(0));\n}\n")
            .Expecting(Asserts(0, "EndTheWorld", "Assert.That"))
            .RunAsync();

    [AcceptedCase]
    public Task a_helper_that_builds_a_page_and_performs_a_gesture_is_accepted()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile(
                "Rounds.cs",
                "internal static class Rounds\n{\n"
                + "    internal static ILocator Rows(IPage page) => page.GetByRole(AriaRole.Listitem);\n\n"
                + "    internal static Task Open(IPage page) => page.GotoAsync(\"/rounds\");\n\n"
                + "    internal static void Expect(int beds) { }\n\n"
                + "    internal static void Count() => Expect(4);\n\n"
                + "    internal static void Wait() => Assertions.SetDefaultExpectTimeout(5000);\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_assert_in_a_helper_is_refused_in_a_project_with_no_playwright()
        => new Case<TEST011OnlyATestAsserts>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static void Counted(int beds) => Assert.{|#0:That|}(beds, Is.EqualTo(4)); }\n")
            .Expecting(Asserts(0, "Counted", "Assert.That"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_is_not_read()
        => new Case<TEST011OnlyATestAsserts>()
            .Without("nunit.framework.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static Task Listed(ILocator rows) => Expect(rows).ToHaveCountAsync(2); }\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST011OnlyATestAsserts>()
            .WithFile("Rounds.cs", "internal static class Rounds { internal static Task Listed(ILocator rows) => {|CS0103:Expects|}(rows).ToHaveCountAsync(2); }\n")
            .RunAsync();

    private static DiagnosticResult Asserts(int at, string member, string with)
        => new DiagnosticResult(TEST011OnlyATestAsserts.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{member}' asserts with '{with}' and is no test; a helper builds a page or performs a gesture, so move the assertion into the test it proves");
}
