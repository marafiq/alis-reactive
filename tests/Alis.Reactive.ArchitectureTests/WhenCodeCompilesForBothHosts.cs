using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenCodeCompilesForBothHosts
{
    private const string Net48File = ".Net48.cs";
    private const string Net10File = ".Net10.cs";
    private const string OneHostsFile = "what one host alone has is a file of its own, named \"" + Net48File + "\" or \"" + Net10File + "\" and wrapped whole";

    private static readonly (string Symbol, string Suffix)[] HostFiles = [(Hosts.Net48, Net48File), (Hosts.Net10, Net10File)];

    private sealed record Written(string File, string Text);

    [Test]
    public void a_shipped_packages_file_compiles_for_both_hosts_or_is_one_hosts_whole()
    {
        var shipped = Repository.TrackedFiles
            .Where(file => file.Split('/') is [_, "PublicAPI", ..])
            .Select(file => file[..(file.IndexOf('/') + 1)])
            .ToHashSet(StringComparer.Ordinal);
        var sources = Repository.TrackedFiles
            .Where(file => file.EndsWith(".cs", StringComparison.Ordinal) && shipped.Any(package => file.StartsWith(package, StringComparison.Ordinal)))
            .Select(file => new Written(file, File.ReadAllText(Path.Combine(Repository.Root, file))))
            .ToList();

        Assert.That(shipped, Is.Not.Empty, "No folder holding a PublicAPI folder was found");
        Assert.That(sources, Is.Not.Empty, "No tracked C# of a shipped package was found");
        Assert.That(Problems(sources), Is.Empty, "A file compiles for both hosts or for one: " + OneHostsFile + ".");
    }

    [RefusedCase]
    public void a_directive_inside_a_file_is_refused_at_its_line()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\npublic sealed class Nurse\n{\n#if NET48\n    public int Rounds => 4;\n#else\n    public int Rounds => 10;\n#endif\n}\n")]),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.cs:4: #if inside the file; " + OneHostsFile,
                "Ward/Nurse.cs:6: #else inside the file; " + OneHostsFile,
                "Ward/Nurse.cs:8: #endif inside the file; " + OneHostsFile,
            }));

    [RefusedCase]
    public void a_directive_that_opens_after_the_first_line_or_closes_before_the_last_is_refused()
        => Assert.That(
            Problems(
            [
                new("Ward/Carer.cs", "namespace Ward;\n#if NET48\ninternal sealed class Carer;\n#endif\n"),
                new("Ward/Porter.cs", "#if NET48\nnamespace Ward;\n#endif\ninternal sealed class Porter;\n"),
            ]),
            Is.EqualTo(new[]
            {
                "Ward/Carer.cs:2: #if inside the file; " + OneHostsFile,
                "Ward/Carer.cs:4: #endif inside the file; " + OneHostsFile,
                "Ward/Porter.cs:1: #if inside the file; " + OneHostsFile,
                "Ward/Porter.cs:3: #endif inside the file; " + OneHostsFile,
            }));

    [RefusedCase]
    public void a_second_branch_in_a_wrapped_file_is_refused_at_its_line()
        => Assert.That(
            Problems([new("Ward/Nurse.Net48.cs", "#if NET48\nnamespace Ward;\ninternal sealed class Nurse;\n#elif NET10_0_OR_GREATER\nnamespace Ward;\ninternal sealed class Carer;\n#endif\n")]),
            Is.EqualTo(new[] { "Ward/Nurse.Net48.cs:4: #elif inside the file; " + OneHostsFile }));

    [RefusedCase]
    public void a_file_wrapped_in_another_condition_is_refused()
        => Assert.That(
            Problems(
            [
                new("Ward/Nurse.Net10.cs", "#if !NET48\nnamespace Ward;\ninternal sealed class Nurse;\n#endif\n"),
                new("Ward/Carer.Net10.cs", "#if NET6_0_OR_GREATER\nnamespace Ward;\ninternal sealed class Carer;\n#endif\n"),
            ]),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.Net10.cs:1: wrapped in #if !NET48; a one-host file is wrapped in #if NET48 or #if NET10_0_OR_GREATER",
                "Ward/Carer.Net10.cs:1: wrapped in #if NET6_0_OR_GREATER; a one-host file is wrapped in #if NET48 or #if NET10_0_OR_GREATER",
            }));

    [RefusedCase]
    public void a_wrapped_file_not_named_for_its_host_is_refused()
        => Assert.That(
            Problems(
            [
                new("Ward/Nurse.cs", "#if NET48\nnamespace Ward;\ninternal sealed class Nurse;\n#endif\n"),
                new("Ward/Carer.Net48.cs", "#if NET10_0_OR_GREATER\nnamespace Ward;\ninternal sealed class Carer;\n#endif\n"),
            ]),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.cs: wrapped whole in #if NET48 and not named \".Net48.cs\"",
                "Ward/Carer.Net48.cs: wrapped whole in #if NET10_0_OR_GREATER and not named \".Net10.cs\"",
            }));

    [RefusedCase]
    public void a_file_named_for_a_host_and_not_wrapped_is_refused()
        => Assert.That(
            Problems([new("Ward/Nurse.Net10.cs", "namespace Ward;\ninternal sealed class Nurse;\n")]),
            Is.EqualTo(new[] { "Ward/Nurse.Net10.cs: named for one host and not wrapped whole in #if NET10_0_OR_GREATER" }));

    [AcceptedCase]
    public void a_file_for_both_hosts_and_a_file_for_each_host_are_accepted()
        => Assert.That(
            Problems(
            [
                new("Ward/Nurse.cs", "#nullable enable\nnamespace Ward;\ninternal sealed partial class Nurse\n{\n    internal partial int Rounds();\n}\n"),
                new("Ward/Nurse.Net48.cs", "#if NET48\nnamespace Ward;\ninternal sealed partial class Nurse\n{\n    internal partial int Rounds() => 4;\n}\n#endif\n"),
                new("Ward/Nurse.Net10.cs", "\n#if NET10_0_OR_GREATER\nnamespace Ward;\ninternal sealed partial class Nurse\n{\n    internal partial int Rounds() => 10;\n}\n#endif\n\n"),
            ]),
            Is.Empty);

    private static List<string> Problems(IReadOnlyList<Written> sources) => sources.SelectMany(ProblemsOf).ToList();

    private static IEnumerable<string> ProblemsOf(Written source)
    {
        var root = CSharpSyntaxTree.ParseText(source.Text, new CSharpParseOptions(LanguageVersion.CSharp14), source.File).GetRoot();
        var conditionals = Conditionals(root).ToList();
        var named = HostFiles.Where(host => source.File.EndsWith(host.Suffix, StringComparison.Ordinal)).ToList();
        if (conditionals.Count == 0)
            return named.Select(host => $"{source.File}: named for one host and not wrapped whole in #if {host.Symbol}");

        var opening = conditionals[0] as IfDirectiveTriviaSyntax;
        var closing = conditionals[^1] as EndIfDirectiveTriviaSyntax;
        var wrapsTheFile = opening != null && closing != null
                           && string.IsNullOrWhiteSpace(source.Text[..opening.SpanStart])
                           && string.IsNullOrWhiteSpace(source.Text[closing.Span.End..]);
        if (!wrapsTheFile)
            return conditionals.Select(directive => $"{source.File}:{LineOf(directive)}: {NameOf(directive)} inside the file; {OneHostsFile}");

        var inside = conditionals.Skip(1).SkipLast(1).Select(directive => $"{source.File}:{LineOf(directive)}: {NameOf(directive)} inside the file; {OneHostsFile}").ToList();
        if (inside.Count > 0) return inside;

        var condition = opening!.Condition.ToString();
        var wrapped = HostFiles.Where(host => host.Symbol == condition).ToList();
        if (wrapped.Count == 0)
            return [$"{source.File}:{LineOf(opening)}: wrapped in #if {condition}; a one-host file is wrapped in #if {Hosts.Net48} or #if {Hosts.Net10}"];

        return wrapped.Where(host => !source.File.EndsWith(host.Suffix, StringComparison.Ordinal))
            .Select(host => $"{source.File}: wrapped whole in #if {host.Symbol} and not named \"{host.Suffix}\"");
    }

    private static IEnumerable<DirectiveTriviaSyntax> Conditionals(SyntaxNode root)
    {
        for (var directive = root.GetFirstDirective(); directive != null; directive = directive.GetNextDirective())
        {
            if (directive is IfDirectiveTriviaSyntax or ElifDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax)
                yield return directive;
        }
    }

    private static int LineOf(DirectiveTriviaSyntax directive) => directive.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static string NameOf(DirectiveTriviaSyntax directive) => "#" + directive.DirectiveNameToken.Text;
}
