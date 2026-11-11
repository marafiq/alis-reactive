using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAnArchitectureTestIsWritten
{
    private const string ArchitectureTests = "tests/Alis.Reactive.ArchitectureTests/";
    private const string Rule = "Test";
    private const string RefusedCase = "RefusedCase";
    private const string AcceptedCase = "AcceptedCase";

    private static readonly (string Rules, string Cases)[] RulesProjects =
    [
        ("Alis.Reactive.Analyzers/", "tests/Alis.Reactive.AnalyzerTests/"),
        ("tests/Alis.Reactive.TestRules/", "tests/Alis.Reactive.TestRules.AnalyzerTests/"),
    ];

    private sealed record Written(string File, string Text);

    [Test]
    public void every_fixture_that_holds_a_rule_holds_a_refused_and_an_accepted_case()
    {
        var fixtures = Tracked(ArchitectureTests);

        Assert.That(fixtures, Is.Not.Empty, "No architecture test was found");
        AssertNone(RulesWithoutCases(fixtures), "A rule that only scans a clean repository passes whether it works or not. Give its fixture a case of each mark:");
    }

    [Test]
    public void every_analyzer_has_a_refused_and_an_accepted_case_that_runs_it()
    {
        foreach (var (rules, cases) in RulesProjects)
        {
            var analyzers = Tracked(rules);

            Assert.That(AnalyzersIn(analyzers), Is.Not.Empty, "No analyzer was found under " + rules);
            AssertNone(AnalyzersWithoutCases(analyzers, Tracked(cases)), "An analyzer no case runs is unproven. Give it a refused and an accepted case under " + cases);
        }
    }

    [RefusedCase]
    public void a_fixture_that_holds_a_rule_and_lacks_a_mark_is_refused()
        => Assert.That(
            RulesWithoutCases(
            [
                new("WhenAWardOpens.cs", "[TestFixture] public class WhenAWardOpens { [Test] public void every_bed_is_made() { } [AcceptedCase] public void a_made_bed_is_accepted() { } }"),
                new("WhenAWardCloses.cs", "[TestFixture] public class WhenAWardCloses { [Test] public void every_door_is_shut() { } [RefusedCase] public void an_open_door_is_refused() { } }"),
            ]),
            Is.EqualTo(new[] { "WhenAWardCloses holds a rule and no accepted case", "WhenAWardOpens holds a rule and no refused case" }));

    [AcceptedCase]
    public void a_fixture_with_a_case_of_each_mark_and_a_fixture_with_no_rule_are_accepted()
        => Assert.That(
            RulesWithoutCases(
            [
                new("WhenAWardOpens.cs", "[TestFixture] public class WhenAWardOpens { [Test] public void every_bed_is_made() { } [RefusedCase] public void an_unmade_bed_is_refused() { } [AcceptedCase] public void a_made_bed_is_accepted() { } }"),
                new("WhenARuleReadsAWard.cs", "[TestFixture] public class WhenARuleReadsAWard { [RefusedCase] public void an_unmade_bed_is_refused() { } }"),
                new("Beds.cs", "internal static class Beds { internal static int Count => 0; }"),
            ]),
            Is.Empty);

    [RefusedCase]
    public void an_analyzer_or_a_suppressor_no_case_runs_is_refused()
        => Assert.That(
            AnalyzersWithoutCases(
                [
                    new("WARD001BedIsMade.cs", "[DiagnosticAnalyzer(LanguageNames.CSharp)] public sealed class WARD001BedIsMade : DiagnosticAnalyzer { }"),
                    new("WARD002QuietHours.cs", "[DiagnosticAnalyzer(LanguageNames.CSharp)] public sealed class WARD002QuietHours : DiagnosticSuppressor { }"),
                ],
                [
                    new("WhenABedIsMade.cs", "[TestFixture] public class WhenABedIsMade { [AcceptedCase] public Task a_made_bed_is_accepted() => new Case<WARD001BedIsMade>().RunAsync(); }"),
                ]),
            Is.EqualTo(new[] { "WARD001BedIsMade has no refused case that runs it", "WARD002QuietHours has no accepted case that runs it", "WARD002QuietHours has no refused case that runs it" }));

    [AcceptedCase]
    public void an_analyzer_a_fixture_runs_under_both_marks_is_accepted()
        => Assert.That(
            AnalyzersWithoutCases(
                [new("WARD001BedIsMade.cs", "[DiagnosticAnalyzer(LanguageNames.CSharp)] public sealed class WARD001BedIsMade : DiagnosticAnalyzer { }")],
                [
                    new("WhenABedIsMade.cs", "[TestFixture] public class WhenABedIsMade { [RefusedCase] public Task an_unmade_bed_is_refused() => Bed(); [AcceptedCase] public Task a_made_bed_is_accepted() => Bed(); private static Task Bed() => new Case<WARD001BedIsMade>().RunAsync(); }"),
                ]),
            Is.Empty);

    private static List<Written> Tracked(string folder)
        => Repository.TrackedFiles
            .Where(file => file.StartsWith(folder, StringComparison.Ordinal) && file.EndsWith(".cs", StringComparison.Ordinal))
            .Select(file => new Written(file, File.ReadAllText(Path.Combine(Repository.Root, file))))
            .ToList();

    private static void AssertNone(IReadOnlyCollection<string> problems, string header)
        => Assert.That(problems, Is.Empty, header + "\n" + string.Join("\n", problems));

    private static List<string> RulesWithoutCases(IReadOnlyList<Written> sources)
        => Fixtures(sources)
            .Where(fixture => Marked(fixture, Rule).Any())
            .SelectMany(fixture => new[]
            {
                Marked(fixture, RefusedCase).Any() ? null : $"{fixture.Identifier.ValueText} holds a rule and no refused case",
                Marked(fixture, AcceptedCase).Any() ? null : $"{fixture.Identifier.ValueText} holds a rule and no accepted case",
            })
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToList();

    private static List<string> AnalyzersWithoutCases(IReadOnlyList<Written> rules, IReadOnlyList<Written> cases)
    {
        var fixtures = Fixtures(cases).ToList();
        return AnalyzersIn(rules)
            .SelectMany(analyzer => new[]
            {
                fixtures.Any(fixture => Runs(fixture, analyzer) && Marked(fixture, RefusedCase).Any()) ? null : $"{analyzer} has no refused case that runs it",
                fixtures.Any(fixture => Runs(fixture, analyzer) && Marked(fixture, AcceptedCase).Any()) ? null : $"{analyzer} has no accepted case that runs it",
            })
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> AnalyzersIn(IReadOnlyList<Written> sources)
        => Classes(sources).Where(declared => Carries(declared.AttributeLists, "DiagnosticAnalyzer")).Select(declared => declared.Identifier.ValueText).ToList();

    private static IEnumerable<ClassDeclarationSyntax> Fixtures(IReadOnlyList<Written> sources)
        => Classes(sources).Where(declared => Carries(declared.AttributeLists, "TestFixture"));

    private static IEnumerable<ClassDeclarationSyntax> Classes(IReadOnlyList<Written> sources)
        => sources.SelectMany(source => CSharpSyntaxTree.ParseText(source.Text, new CSharpParseOptions(LanguageVersion.CSharp14), source.File)
            .GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

    private static IEnumerable<MethodDeclarationSyntax> Marked(ClassDeclarationSyntax fixture, string mark)
        => fixture.Members.OfType<MethodDeclarationSyntax>().Where(method => Carries(method.AttributeLists, mark));

    private static bool Runs(ClassDeclarationSyntax fixture, string analyzer)
        => fixture.DescendantNodes().OfType<TypeArgumentListSyntax>()
            .SelectMany(arguments => arguments.Arguments.OfType<IdentifierNameSyntax>())
            .Any(argument => argument.Identifier.ValueText == analyzer);

    private static bool Carries(SyntaxList<AttributeListSyntax> lists, string attribute)
        => lists.SelectMany(list => list.Attributes).Any(applied => applied.Name.ToString() is var name && (name == attribute || name == attribute + "Attribute"));
}
