using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenTheCoreIsCompiled
{
    private const string Core = "Alis.Reactive/";
    private const string Banned = "RS0030";
    private const string OnlyWhereListed = "the core reads type information only where the owner's list names the file and the member (spec/Core/DESIGN.md, D18.1)";

    private static readonly string[] TypeReadsTheOwnerLists = [];

    private static readonly string[] HoldersOfTypeInformation =
    [
        "System.Type",
        "System.Activator",
        "System.Delegate",
        "System.ComponentModel.TypeDescriptor",
    ];

    private sealed record Written(string File, string Text);

    private sealed record Read(string File, int Line, string Member)
    {
        internal string Listed => File + " " + Member;
    }

    [Test]
    public void the_core_reads_type_information_only_where_the_owner_lists_it()
    {
        var sources = Repository.TrackedFiles
            .Where(file => file.StartsWith(Core, StringComparison.Ordinal) && file.EndsWith(".cs", StringComparison.Ordinal))
            .Select(file => new Written(file[Core.Length..], File.ReadAllText(Path.Combine(Repository.Root, file))))
            .ToList();

        Assert.That(sources, Is.Not.Empty, "No tracked C# of the core was found");
        Assert.That(Problems(sources, TypeReadsTheOwnerLists), Is.Empty, "A read of type information outside the owner's list: " + OnlyWhereListed + ".");
    }

    [RefusedCase]
    public void a_member_of_a_type_is_refused_where_it_is_read()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static int Members() => typeof(Nurse).GetProperties().Length;\n}\n")], []),
            Is.EqualTo(new[] { "Ward/Nurse.cs:4: reads System.Type.GetProperties; " + OnlyWhereListed }));

    [RefusedCase]
    public void a_type_of_reflection_and_its_member_are_refused_where_they_are_named()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static string Named(System.Reflection.MemberInfo member)\n        => member.Name;\n}\n")], []),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.cs:4: reads System.Reflection.MemberInfo; " + OnlyWhereListed,
                "Ward/Nurse.cs:5: reads System.Reflection.MemberInfo.Name; " + OnlyWhereListed,
            }));

    [RefusedCase]
    public void a_member_of_the_activator_of_a_delegate_and_of_the_type_descriptor_is_refused()
        => Assert.That(
            Problems(
            [
                new(
                    "Ward/Nurse.cs",
                    "namespace Ward;\ninternal static class Nurse\n{\n    internal static object? Made() => System.Activator.CreateInstance(typeof(Nurse));\n    internal static object? Held(System.Action step) => step.Target;\n    internal static int Described() => System.ComponentModel.TypeDescriptor.GetProperties(typeof(Nurse)).Count;\n}\n"),
            ],
            []),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.cs:4: reads System.Activator.CreateInstance; " + OnlyWhereListed,
                "Ward/Nurse.cs:5: reads System.Delegate.Target; " + OnlyWhereListed,
                "Ward/Nurse.cs:6: reads System.ComponentModel.TypeDescriptor.GetProperties; " + OnlyWhereListed,
            }));

    [RefusedCase]
    public void a_read_the_list_names_for_another_file_is_refused()
        => Assert.That(
            Problems(
            [
                new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static string? Id() => typeof(Nurse).FullName;\n}\n"),
                new("Ward/Carer.cs", "namespace Ward;\ninternal static class Carer\n{\n    internal static string? Id() => typeof(Carer).FullName;\n}\n"),
            ],
            ["Ward/Nurse.cs System.Type.FullName"]),
            Is.EqualTo(new[] { "Ward/Carer.cs:4: reads System.Type.FullName; " + OnlyWhereListed }));

    [RefusedCase]
    public void a_listed_read_the_core_no_longer_holds_is_refused()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static string Id() => nameof(Nurse);\n}\n")], ["Ward/Nurse.cs System.Type.FullName"]),
            Is.EqualTo(new[] { "the owner's list names 'Ward/Nurse.cs System.Type.FullName', which the core does not read: take it off the list" }));

    [RefusedCase]
    public void a_file_that_does_not_compile_is_refused_since_its_reads_are_not_read()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static int Members() => rounds.GetType().GetProperties().Length;\n}\n")], []),
            Is.EqualTo(new[] { "Ward/Nurse.cs:4: CS0103: the file does not compile here, so what it reads is not read" }));

    [AcceptedCase]
    public void a_types_name_read_where_the_list_names_it_is_accepted()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static string? Id() => typeof(Nurse).FullName;\n}\n")], ["Ward/Nurse.cs System.Type.FullName"]),
            Is.Empty);

    [AcceptedCase]
    public void a_type_passed_and_compared_as_itself_is_accepted()
        => Assert.That(
            Problems([new("Ward/Nurse.cs", "namespace Ward;\ninternal static class Nurse\n{\n    internal static bool IsANurse(System.Type staff) => staff == typeof(Nurse);\n}\n")], []),
            Is.Empty);

    [RefusedCase]
    public async Task each_use_the_core_bans_is_refused_by_the_build()
        => Assert.That(
            await BannedUses(new(
                "Ward/Nurse.cs",
                "namespace Ward;\n" +
                "internal static class Nurse\n" +
                "{\n" +
                "    internal static string? Housed(System.Type staff) => staff.Assembly.FullName;\n" +
                "    internal static object? Made() => System.Activator.CreateInstance(typeof(Nurse));\n" +
                "    internal static System.Type? Found() => System.Type.GetType(\"Ward.Nurse\");\n" +
                "    internal static object Run(System.Linq.Expressions.LambdaExpression read) => read.Compile();\n" +
                "    internal static object Interpreted(System.Linq.Expressions.LambdaExpression read) => read.Compile(true);\n" +
                "    internal static System.Func<int, bool> Typed(System.Linq.Expressions.Expression<System.Func<int, bool>> isOfAge) => isOfAge.Compile();\n" +
                "    internal static System.Func<int, bool> TypedInterpreted(System.Linq.Expressions.Expression<System.Func<int, bool>> isOfAge) => isOfAge.Compile(true);\n" +
                "    internal static object Traced(System.Linq.Expressions.LambdaExpression read, System.Runtime.CompilerServices.DebugInfoGenerator lines) => read.Compile(lines);\n" +
                "    internal static System.Func<int, bool> TypedTraced(System.Linq.Expressions.Expression<System.Func<int, bool>> isOfAge, System.Runtime.CompilerServices.DebugInfoGenerator lines) => isOfAge.Compile(lines);\n" +
                "    internal static bool Matches(string name) => System.Text.RegularExpressions.Regex.IsMatch(name, \"^A\");\n" +
                "}\n")),
            Is.EqualTo(new[]
            {
                "Ward/Nurse.cs:4: " + Banned,
                "Ward/Nurse.cs:5: " + Banned,
                "Ward/Nurse.cs:6: " + Banned,
                "Ward/Nurse.cs:7: " + Banned,
                "Ward/Nurse.cs:8: " + Banned,
                "Ward/Nurse.cs:9: " + Banned,
                "Ward/Nurse.cs:10: " + Banned,
                "Ward/Nurse.cs:11: " + Banned,
                "Ward/Nurse.cs:12: " + Banned,
                "Ward/Nurse.cs:13: " + Banned,
            }));

    [AcceptedCase]
    public async Task a_types_name_and_an_expression_that_is_read_are_left_to_the_core()
        => Assert.That(
            await BannedUses(new(
                "Ward/Nurse.cs",
                "namespace Ward;\n" +
                "internal static class Nurse\n" +
                "{\n" +
                "    internal static string? Id() => typeof(Nurse).FullName;\n" +
                "    internal static System.Linq.Expressions.ExpressionType Kind(System.Linq.Expressions.Expression<System.Func<int, bool>> isOfAge) => isOfAge.Body.NodeType;\n" +
                "}\n")),
            Is.Empty);

    private static List<string> Problems(IReadOnlyList<Written> sources, IReadOnlyList<string> listed)
    {
        var compilation = Compile(sources);
        var broken = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => $"{diagnostic.Location.SourceTree?.FilePath}:{LineOf(diagnostic.Location)}: {diagnostic.Id}: the file does not compile here, so what it reads is not read")
            .ToList();
        if (broken.Count > 0) return broken;

        var reads = compilation.SyntaxTrees.SelectMany(tree => ReadsOf(tree, compilation.GetSemanticModel(tree))).ToList();
        var held = reads.Select(read => read.Listed).ToHashSet(StringComparer.Ordinal);
        var unlisted = reads.Where(read => !listed.Contains(read.Listed)).Select(read => $"{read.File}:{read.Line}: reads {read.Member}; {OnlyWhereListed}");
        var stale = listed.Where(entry => !held.Contains(entry)).Select(entry => $"the owner's list names '{entry}', which the core does not read: take it off the list");
        return [.. unlisted, .. stale];
    }

    private static IEnumerable<Read> ReadsOf(SyntaxTree tree, SemanticModel model)
        => tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>()
            .Select(name => (Name: name, Member: TypeInformationOf(model.GetSymbolInfo(name).Symbol)))
            .Where(read => read.Member != null)
            .Select(read => new Read(tree.FilePath, LineOf(read.Name.GetLocation()), read.Member!));

    private static string? TypeInformationOf(ISymbol? symbol) => symbol switch
    {
        INamedTypeSymbol type when IsOfReflection(type) => type.ToDisplayString(),
        IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol when symbol.ContainingType is { } holder && HoldsTypeInformation(holder)
            => $"{holder.OriginalDefinition.ToDisplayString()}.{symbol.Name}",
        _ => null,
    };

    private static bool HoldsTypeInformation(INamedTypeSymbol holder)
        => IsOfReflection(holder) || HoldersOfTypeInformation.Contains(holder.OriginalDefinition.ToDisplayString());

    private static bool IsOfReflection(INamedTypeSymbol type)
    {
        var space = type.ContainingNamespace.ToDisplayString();
        return space == "System.Reflection" || space.StartsWith("System.Reflection.", StringComparison.Ordinal);
    }

    private static async Task<List<string>> BannedUses(Written source)
    {
        var bannedByTheCore = new BannedSymbols(File.ReadAllText(Path.Combine(Repository.Root, Core + "BannedSymbols.txt")));
        var analyzed = Compile([source]).WithAnalyzers([new CSharpSymbolIsBannedAnalyzer()], new AnalyzerOptions([bannedByTheCore]));
        var diagnostics = await analyzed.GetAnalyzerDiagnosticsAsync();
        return diagnostics
            .Select(diagnostic => (Line: LineOf(diagnostic.Location), Refusal: $"{diagnostic.Location.SourceTree?.FilePath}:{LineOf(diagnostic.Location)}: {diagnostic.Id}"))
            .Distinct()
            .OrderBy(refused => refused.Line)
            .Select(refused => refused.Refusal)
            .ToList();
    }

    private static CSharpCompilation Compile(IReadOnlyList<Written> sources)
    {
        var parse = new CSharpParseOptions(LanguageVersion.CSharp14, preprocessorSymbols: Hosts.SymbolsOfNet10);
        var trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Text, parse, source.File));
        var references = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "core-references.txt")).Select(path => MetadataReference.CreateFromFile(path));
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);
        return CSharpCompilation.Create("Alis.Reactive", trees, references, options);
    }

    private static int LineOf(Location location) => location.GetLineSpan().StartLinePosition.Line + 1;

    private sealed class BannedSymbols(string text) : AdditionalText
    {
        public override string Path => "BannedSymbols.txt";

        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
    }
}
