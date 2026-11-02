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

    /// <summary>Where a package fixture's files and its slice table sit; the analyzer takes the table's directory as the package root.</summary>
    public const string PackageRoot = "/pkg/";

    /// <summary>
    /// The analyzer, run over a package laid out under <see cref="PackageRoot"/> with its slice table, reports
    /// exactly one diagnostic: this id and message, on the one span marked <c>[|like this|]</c> in the table or in a
    /// file; with no mark, a diagnostic with no location. A null table is a package without one.
    /// </summary>
    public static async Task ReportsInPackage<TAnalyzer>(string assemblyName, string? table, string id, string message, params (string Path, string Source)[] files)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var package = UnmarkPackage(table, files);
        var reported = await AnalyzePackage<TAnalyzer>(assemblyName, package.Table, package.Files);

        Assert.That(
            reported.Select(diagnostic => DescribeInPackage(diagnostic.Id, diagnostic.Location.GetLineSpan().Path ?? "", diagnostic.Location.SourceSpan, diagnostic.GetMessage())),
            Is.EqualTo(new[] { DescribeInPackage(id, package.MarkedPath, package.MarkedSpan, message) }));
    }

    /// <summary>The analyzer, run over a package laid out under <see cref="PackageRoot"/>, reports nothing.</summary>
    public static async Task ReportsNothingInPackage<TAnalyzer>(string assemblyName, string? table, params (string Path, string Source)[] files)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var package = UnmarkPackage(table, files);
        var reported = await AnalyzePackage<TAnalyzer>(assemblyName, package.Table, package.Files);

        Assert.That(
            reported.Select(diagnostic => DescribeInPackage(diagnostic.Id, diagnostic.Location.GetLineSpan().Path ?? "", diagnostic.Location.SourceSpan, diagnostic.GetMessage())),
            Is.Empty);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzePackage<TAnalyzer>(string assemblyName, string? table, (string Path, string Source)[] files)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            files.Select(file => CSharpSyntaxTree.ParseText(file.Source, path: PackageRoot + file.Path)),
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        Assert.That(errors, Is.Empty, "The fixture must compile.\n" + string.Join("\n", errors));

        var options = table == null ? null : new AnalyzerOptions([new TableText(PackageRoot + "SliceTree.txt", table)]);
        return await compilation
            .WithAnalyzers([new TAnalyzer()], options)
            .GetAnalyzerDiagnosticsAsync();
    }

    private static string DescribeInPackage(string id, string path, TextSpan span, string message) =>
        $"{id} {path} ({span.Start}..{span.End}) {message}";

    // At most one of the table and the files carries the mark; the marked text and its span name the expected location.
    private static (string? Table, (string Path, string Source)[] Files, string MarkedPath, TextSpan MarkedSpan) UnmarkPackage(string? table, (string Path, string Source)[] files)
    {
        var markedPath = "";
        var markedSpan = new TextSpan(0, 0);
        if (table != null && table.Contains("[|"))
        {
            (table, markedSpan) = Unmark(table);
            markedPath = PackageRoot + "SliceTree.txt";
        }
        var unmarked = new (string Path, string Source)[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            var (path, source) = files[i];
            if (source.Contains("[|"))
            {
                Assert.That(markedPath, Is.Empty, "A package fixture marks one span.");
                (source, markedSpan) = Unmark(source);
                markedPath = PackageRoot + path;
            }
            unmarked[i] = (path, source);
        }
        return (table, unmarked, markedPath, markedSpan);
    }

    // A slice table handed to the analyzer the way the compiler hands an AdditionalFiles item.
    private sealed class TableText(string path, string text) : AdditionalText
    {
        public override string Path => path;

        public override SourceText? GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
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
