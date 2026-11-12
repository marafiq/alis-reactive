using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestExpectsWhatItTyped
{
    private const string Opening = "public sealed class WhenANurseIsNamed\n{\n    private readonly IPage _page = null!;\n    private readonly ILocator _search = null!;\n    private readonly ILocator _other = null!;\n    private static readonly ILocator Shared = null!;\n\n    private ILocator Nurse => _page.GetByLabel(\"Nurse\");\n    private ILocator Carer => _page.GetByLabel(\"Carer\");\n\n    [Test]\n    public async Task the_nurse_is_greeted()\n    {\n";
    private const string Closing = "\n    }\n}\n";
    private const string Rounds = "internal static class Rounds\n{\n    internal static string Next() => \"Helen\";\n    internal static Task TypeName(ILocator name) => name.FillAsync(\"Helen\");\n    internal static ILocatorAssertions Expecting(ILocator name) => Expect(name);\n}\ninternal sealed class Form\n{\n    internal ILocator Name = null!;\n    internal ILocator Ward => Name;\n}\n";

    [RefusedCase]
    public Task a_value_the_test_typed_and_expects_of_the_same_local_is_refused_at_the_value()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        var name = _page.GetByLabel(\"Name\");\n"
                + "        await name.FillAsync(\"Helen\");\n"
                + "        await Expect(name).ToHaveValueAsync({|#0:\"Helen\"|});"
                + Closing)
            .Expecting(Arrange(0, "Helen"))
            .RunAsync();

    [AcceptedCase]
    public Task what_the_page_makes_of_the_typed_value_and_the_value_in_another_locator_are_accepted()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        var name = _page.GetByLabel(\"Name\");\n"
                + "        var greeting = _page.GetByRole(AriaRole.Status);\n"
                + "        await name.FillAsync(\"Helen\");\n"
                + "        await Expect(name).ToHaveValueAsync(\"HELEN\");\n"
                + "        await Expect(greeting).ToHaveTextAsync(\"Helen\");\n"
                + "        await Expect(_page.GetByLabel(\"Ward\")).ToHaveValueAsync(\"Helen\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveCountAsync(1);"
                + Closing)
            .RunAsync();

    [RefusedCase]
    public Task the_same_expression_field_and_property_are_the_same_locator()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        const string Typed = \"Mia\";\n"
                + "        await _page.GetByLabel(\"Ward\").PressSequentiallyAsync(\"North\");\n"
                + "        await Expect(_page.GetByLabel(\"Ward\")).ToHaveValueAsync({|#0:\"North\"|});\n"
                + "        await _search.FillAsync(\"Ada\");\n"
                + "        await Expect(this._search).ToHaveValueAsync({|#1:\"Ada\"|});\n"
                + "        await Nurse.FillAsync(Typed);\n"
                + "        await Expect(this.Nurse).Not.ToHaveTextAsync({|#2:\"Mia\"|});\n"
                + "        await Expect(Nurse).ToHaveValuesAsync([\"Ben\", {|#3:Typed|}]);\n"
                + "        await Expect(Nurse).ToHaveValuesAsync(new[] { {|#4:\"Mia\"|} });\n"
                + "        await _other.FillAsync(options: null, value: \"Zoe\");\n"
                + "        await Expect(_other).ToHaveAttributeAsync(\"value\", {|#5:\"Zoe\"|});\n"
                + "        await Shared.FillAsync(\"Ivy\");\n"
                + "        await Expect(Shared).ToHaveValueAsync({|#6:\"Ivy\"|});"
                + Closing)
            .Expecting(Arrange(0, "North"), Arrange(1, "Ada"), Arrange(2, "Mia"), Arrange(3, "Mia"), Arrange(4, "Mia"), Arrange(5, "Zoe"), Arrange(6, "Ivy"))
            .RunAsync();

    [AcceptedCase]
    public Task another_field_another_local_and_another_expression_are_other_locators()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        var first = _page.GetByLabel(\"Name\");\n"
                + "        var second = _page.GetByLabel(\"Name\");\n"
                + "        await first.FillAsync(\"Helen\");\n"
                + "        await Expect(second).ToHaveValueAsync(\"Helen\");\n"
                + "        await _search.FillAsync(\"Ada\");\n"
                + "        await Expect(_other).ToHaveValueAsync(\"Ada\");\n"
                + "        await Expect(Nurse).ToHaveValueAsync(\"Ada\");\n"
                + "        await _page.GetByLabel(\"Ward\").FillAsync(\"North\");\n"
                + "        await Expect(_page.GetByLabel(\"Wing\")).ToHaveValueAsync(\"North\");\n"
                + "        await Expect(_page).ToHaveURLAsync(\"North\");\n"
                + "        await Carer.FillAsync(\"Bea\");\n"
                + "        await Expect(Nurse).ToHaveValueAsync(\"Bea\");\n"
                + "        {\n            var name = _page.GetByLabel(\"Name\");\n            await name.FillAsync(\"Cal\");\n        }\n"
                + "        {\n            var name = _page.GetByLabel(\"Alias\");\n            await Expect(name).ToHaveValueAsync(\"Cal\");\n        }\n"
                + "        var admission = new Form();\n"
                + "        var discharge = new Form();\n"
                + "        await admission.Name.FillAsync(\"Dee\");\n"
                + "        await Expect(discharge.Name).ToHaveValueAsync(\"Dee\");\n"
                + "        await admission.Ward.FillAsync(\"East\");\n"
                + "        await Expect(discharge.Ward).ToHaveValueAsync(\"East\");"
                + Closing
                + Rounds)
            .RunAsync();

    [RefusedCase]
    public Task a_value_typed_in_a_local_function_of_the_test_is_the_tests_own()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        Task Type() => _search.FillAsync(\"Ada\");\n"
                + "        await Type();\n"
                + "        Func<Task> expect = () => Expect(_search).ToHaveValueAsync({|#0:\"Ada\"|});\n"
                + "        await expect();"
                + Closing)
            .Expecting(Arrange(0, "Ada"))
            .RunAsync();

    [AcceptedCase]
    public Task a_value_set_by_another_gesture_typed_by_a_helper_or_computed_is_accepted()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        var name = _page.GetByLabel(\"Name\");\n"
                + "        var ward = _page.GetByLabel(\"Ward\");\n"
                + "        await ward.SelectOptionAsync(\"North\");\n"
                + "        await Expect(ward).ToHaveValueAsync(\"North\");\n"
                + "        await Rounds.TypeName(name);\n"
                + "        await Expect(name).ToHaveValueAsync(\"Helen\");\n"
                + "        await _search.FillAsync(Rounds.Next());\n"
                + "        await Expect(_search).ToHaveValueAsync(\"Helen\");\n"
                + "        await _other.FillAsync(\"3\");\n"
                + "        await Expect(_other).ToHaveCountAsync(3);\n"
                + "        var held = Expect(_other);\n"
                + "        await held.ToHaveValueAsync(\"3\");\n"
                + "        await Rounds.Expecting(_other).ToHaveValueAsync(\"3\");\n"
                + "        await Expect(_other).ToContainTextAsync(\"3\");\n"
                + "        await _page.FillAsync(\"Fay\", \"x\");\n"
                + "        await Expect(_page).ToHaveURLAsync(\"Fay\");"
                + Closing
                + Rounds)
            .RunAsync();

    [AcceptedCase]
    public Task members_of_another_assembly_by_the_same_names_are_not_read()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenAWardIsNamed.cs",
                "public sealed class WhenAWardIsNamed\n{\n    [Test]\n    public void the_ward_is_named()\n    {\n        Ward.ILocator name = null!;\n        name.FillAsync(\"North\");\n        Ward.Assertions.Expect(name).ToHaveValueAsync(\"North\");\n    }\n}\n"
                + "namespace Ward\n{\n    public interface ILocator { void FillAsync(string value); }\n    public interface ILocatorAssertions { void ToHaveValueAsync(string value); }\n    public static class Assertions { public static ILocatorAssertions Expect(ILocator locator) => null!; }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_method_that_is_no_test_is_not_read()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "Names.cs",
                "internal static class Names\n{\n    internal static async Task Type(ILocator name)\n    {\n        await name.FillAsync(\"Helen\");\n        await Expect(name).ToHaveValueAsync(\"Helen\");\n    }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_holds_no_test()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .Without("nunit.framework.dll")
            .WithFile(
                "Names.cs",
                "internal static class Names\n{\n    internal static async Task the_nurse_is_greeted(ILocator name)\n    {\n        await name.FillAsync(\"Helen\");\n        await Expect(name).ToHaveValueAsync(\"Helen\");\n    }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST009ExpectedValueIsNotTheArrange>()
            .WithFile(
                "WhenANurseIsNamed.cs",
                Opening
                + "        await _search.FillAsync(\"Ada\");\n"
                + "        await Expect(_search).{|CS1061:ToHaveNoValueAsync|}(\"Ada\");\n"
                + "        await Expect({|CS0103:missing|}).ToHaveValueAsync(\"Ada\");"
                + Closing)
            .RunAsync();

    private static DiagnosticResult Arrange(int at, string value)
        => new DiagnosticResult(TEST009ExpectedValueIsNotTheArrange.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'the_nurse_is_greeted' expects \"{value}\", which it typed into the same locator; expect what the page makes of it");
}
