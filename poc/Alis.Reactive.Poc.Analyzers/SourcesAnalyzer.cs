using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SourcesAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "ALIS028";

    private static readonly DiagnosticDescriptor Implemented = new(
        Id,
        "A source is one the framework makes",
        "'{0}' implements ITypedSource<T>, which only the framework's sources implement: read a value through a member's Read, a list's operation or a value read from data",
        Rules.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Implemented];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var source = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Values.ITypedSource`1");
            if (source is null) return;
            start.RegisterSymbolAction(symbol => Check(symbol, source), SymbolKind.NamedType);
        });
    }

    private static void Check(SymbolAnalysisContext context, INamedTypeSymbol source)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, source.ContainingAssembly)) return;
        var implements = type.AllInterfaces.Any(implemented => SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, source));
        if (implements) context.ReportDiagnostic(Diagnostic.Create(Implemented, type.Locations[0], type.Name));
    }
}
