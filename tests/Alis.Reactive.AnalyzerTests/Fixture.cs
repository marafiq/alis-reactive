using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>
/// Compiles one C# file against the framework's own assemblies and runs one analyzer over it. The span a
/// diagnostic must cover is marked <c>[|like this|]</c>. A fixture that does not compile fails outright: an
/// analyzer sees nothing in code that does not bind, so a broken fixture must not pass as a clean one.
/// </summary>
internal static class Fixture
{
    /// <summary>
    /// The path the Razor source generator gives a view's C#: the view's path with every character that is
    /// not a letter or a digit as an underscore, then <c>.g.cs</c> (the sandbox pdb lists
    /// <c>Views_Shared__Layout_cshtml.g.cs</c>). The view-only analyzers key on it.
    /// </summary>
    public const string View =
        "Microsoft.NET.Sdk.Razor.SourceGenerators/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Views_Home_Index_cshtml.g.cs";

    /// <summary>A file that is not a view: the view-only analyzers leave it alone.</summary>
    public const string Controller = "Controllers/HomeController.cs";

    /// <summary>An application's own assembly: not one of the shipped packages whose whole surface ALIS009 polices.</summary>
    public const string AppAssembly = "MapleGrove.Web";

    private static readonly ImmutableArray<MetadataReference> References = TrustedPlatformAssemblies();

    /// <summary>The analyzer reports exactly one diagnostic: this id and message, on the marked span.</summary>
    public static async Task Reports<TAnalyzer>(string id, string message, string path, string markedSource, string assemblyName = AppAssembly)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var (source, span) = Unmark(markedSource);
        var reported = await Analyze<TAnalyzer>(source, path, assemblyName);

        Assert.That(
            reported.Select(diagnostic => Describe(diagnostic.Id, diagnostic.Location.SourceSpan, diagnostic.GetMessage(), source)),
            Is.EqualTo(new[] { Describe(id, span, message, source) }));
    }

    /// <summary>The analyzer reports nothing.</summary>
    public static async Task ReportsNothing<TAnalyzer>(string path, string source, string assemblyName = AppAssembly)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var reported = await Analyze<TAnalyzer>(source, path, assemblyName);

        Assert.That(
            reported.Select(diagnostic => Describe(diagnostic.Id, diagnostic.Location.SourceSpan, diagnostic.GetMessage(), source)),
            Is.Empty);
    }

    private static async Task<ImmutableArray<Diagnostic>> Analyze<TAnalyzer>(string source, string path, string assemblyName)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, path: path)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        Assert.That(errors, Is.Empty, "The fixture must compile.\n" + string.Join("\n", errors));

        return await compilation
            .WithAnalyzers([new TAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }

    private static string Describe(string id, TextSpan span, string message, string source) =>
        $"{id} [|{source.Substring(span.Start, span.Length)}|] ({span.Start}..{span.End}) {message}";

    private static (string Source, TextSpan Span) Unmark(string markedSource)
    {
        var start = markedSource.IndexOf("[|", StringComparison.Ordinal);
        var end = markedSource.IndexOf("|]", StringComparison.Ordinal);
        if (start < 0 || end < start)
            throw new ArgumentException("Mark the span the diagnostic must cover as [|...|].", nameof(markedSource));

        var source = markedSource.Remove(end, 2).Remove(start, 2);
        return (source, TextSpan.FromBounds(start, end - 2));
    }

    // Every assembly this process can load: Microsoft.NETCore.App, Microsoft.AspNetCore.App and the
    // framework's own assemblies, as a view compiles against them.
    private static ImmutableArray<MetadataReference> TrustedPlatformAssemblies()
    {
        var paths = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        return paths
            .Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
    }
}
