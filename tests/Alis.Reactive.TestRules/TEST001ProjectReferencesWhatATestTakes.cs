using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST001ProjectReferencesWhatATestTakes : DiagnosticAnalyzer
{
    public const string Id = "TEST001";

    private const string HandedList = "build_property.AlisTestReferences";
    private const string Title = "A browser test's project references only what a test takes";

    private static readonly ImmutableDictionary<string, string> Kinds = ImmutableDictionary.CreateRange(StringComparer.Ordinal, new[]
    {
        Pair("package", "the package"),
        Pair("framework", "the framework"),
        Pair("project", "the project"),
        Pair("analyzer", "the analyzer"),
        Pair("build", "the project named for its build"),
        Pair("file", "the file"),
    });

    private static readonly ImmutableHashSet<string> Taken = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "package:Microsoft.Playwright",
        "package:NUnit",
        "package:NUnit3TestAdapter",
        "package:Microsoft.NET.Test.Sdk",
        "framework:Microsoft.NETCore.App",
        "project:Alis.Reactive.Conformance.World",
        "analyzer:Alis.Reactive.TestRules",
        "analyzer:Alis.Reactive.Analyzers",
        "build:Alis.Reactive.Conformance.App");

    private static readonly DiagnosticDescriptor Reference = new(
        Id,
        Title,
        "This project references {0} '{1}'; a browser test compiles against Playwright, NUnit and the world contract alone, so remove it",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor NoList = new(
        Id,
        Title,
        "'{0}' takes the test rules and hands the compiler no list of its references; import Alis.Reactive.TestRules.targets after its last reference",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Reference, NoList);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(ReadReferences);
    }

    private static void ReadReferences(CompilationAnalysisContext context)
    {
        var handed = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(HandedList, out var list) ? list.Trim() : "";
        if (handed.Length == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(NoList, Location.None, context.Compilation.AssemblyName));
            return;
        }

        foreach (var entry in handed.Split(',').Select(entry => entry.Trim()).Where(entry => entry.Length > 0 && !Taken.Contains(entry)).OrderBy(entry => entry, StringComparer.Ordinal))
        {
            var colon = entry.IndexOf(':');
            var kind = colon < 0 ? "" : entry.Substring(0, colon);
            context.ReportDiagnostic(Diagnostic.Create(Reference, Location.None, Kinds.TryGetValue(kind, out var said) ? said : "the reference", entry.Substring(colon + 1)));
        }
    }

    private static System.Collections.Generic.KeyValuePair<string, string> Pair(string kind, string said) => new(kind, said);
}
