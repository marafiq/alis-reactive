using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS014DeclarationNamesItsType : DiagnosticAnalyzer
{
    public const string Id = "ALIS014";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "A component or plugin names the TypeScript type it mirrors",
        "Declaration '{0}' names no TypeScript type; add [Mirrors(module, name)] for the type it mirrors",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var component = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Vendors.Component");
        var plugin = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Plugins.Plugin");
        var mirrors = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.MirrorsAttribute");
        if (component is null || plugin is null || mirrors is null || !start.Options.AdditionalFiles.Any(file => VendorContract.IsTable(file.Path))) return;

        start.RegisterSymbolAction(context =>
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!DerivesFrom(type, component) && !DerivesFrom(type, plugin)) return;
            if (type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, mirrors))) return;
            var location = type.Locations.FirstOrDefault(candidate => candidate.IsInSource);
            if (location is not null) context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name));
        }, SymbolKind.NamedType);
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol declaration)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, declaration)) return true;
        }

        return false;
    }
}
