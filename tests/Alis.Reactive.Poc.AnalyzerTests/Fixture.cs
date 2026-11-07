using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

internal static class Fixture
{
    public const string Path = "Components/Ward.cs";

    public const string ContractPath = "obj/Debug/net10.0/vendor-contract.tsv";

    private static readonly ImmutableArray<MetadataReference> References = [.. TrustedPlatformAssemblies()];

    public static async Task Reports<TAnalyzer>(string id, string message, string markedSource, string? contract)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var (source, span) = Unmark(markedSource);
        var reported = await Analyze<TAnalyzer>(source, contract);

        Assert.That(
            reported.Select(diagnostic => Describe(diagnostic.Id, diagnostic.Location.SourceSpan, diagnostic.GetMessage(), source)),
            Is.EqualTo(new[] { Describe(id, span, message, source) }));
    }

    public static async Task ReportsNothing<TAnalyzer>(string source, string? contract)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var reported = await Analyze<TAnalyzer>(source, contract);

        Assert.That(
            reported.Select(diagnostic => Describe(diagnostic.Id, diagnostic.Location.SourceSpan, diagnostic.GetMessage(), source)),
            Is.Empty);
    }

    private static async Task<ImmutableArray<Diagnostic>> Analyze<TAnalyzer>(string source, string? contract)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CSharpCompilation.Create(
            "Ward.Web",
            [CSharpSyntaxTree.ParseText(source, path: Path)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        Assert.That(errors, Is.Empty, "The fixture must compile.\n" + string.Join("\n", errors));

        var options = new AnalyzerOptions(contract is null ? [] : [new ContractText(ContractPath, contract)]);
        return await compilation
            .WithAnalyzers([new TAnalyzer()], options)
            .GetAnalyzerDiagnosticsAsync();
    }

    private sealed class ContractText(string path, string text) : AdditionalText
    {
        public override string Path => path;

        public override SourceText? GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
    }

    private static string Describe(string id, TextSpan span, string message, string source)
        => $"{id} [|{source.Substring(span.Start, span.Length)}|] ({span.Start}..{span.End}) {message}";

    private static (string Source, TextSpan Span) Unmark(string markedSource)
    {
        var start = markedSource.IndexOf("[|", StringComparison.Ordinal);
        var end = markedSource.IndexOf("|]", StringComparison.Ordinal);
        if (start < 0 || end < start)
            throw new ArgumentException("Mark the span the diagnostic must cover as [|...|].", nameof(markedSource));

        var source = markedSource.Remove(end, 2).Remove(start, 2);
        return (source, TextSpan.FromBounds(start, end - 2));
    }

    private static IEnumerable<MetadataReference> TrustedPlatformAssemblies()
    {
        var paths = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        return paths.Split(System.IO.Path.PathSeparator).Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));
    }
}
