using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS012PublicClassIsSealed : DiagnosticAnalyzer
{
    public const string Id = "ALIS012";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "A public class nothing derives from is sealed",
        "'{0}' is public and nothing in the package derives from it; seal it",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Rule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var table = SliceTable.Read(start.Options, start.CancellationToken);
        if (table is null) return;

        var derivedFrom = new ConcurrentDictionary<INamedTypeSymbol, bool>(SymbolEqualityComparer.Default);
        var open = new ConcurrentDictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
        start.RegisterSymbolAction(
            context =>
            {
                var type = (INamedTypeSymbol)context.Symbol;
                if (type.BaseType is { } parent) derivedFrom.TryAdd(parent.OriginalDefinition, true);
                if (IsOpenPublicClass(type) && NameInAReadFile(type, table) is { } name) open.TryAdd(type, name);
            },
            SymbolKind.NamedType);
        start.RegisterCompilationEndAction(end =>
        {
            foreach (var left in open.Where(left => !derivedFrom.ContainsKey(left.Key)))
                end.ReportDiagnostic(Diagnostic.Create(Rule, left.Value, left.Key.ToDisplayString()));
        });
    }

    private static bool IsOpenPublicClass(INamedTypeSymbol type)
        => type is { TypeKind: TypeKind.Class, DeclaredAccessibility: Accessibility.Public, IsSealed: false, IsAbstract: false, IsStatic: false };

    private static Location? NameInAReadFile(INamedTypeSymbol type, SliceTable table)
        => type.Locations
            .Where(location => location.SourceTree is { } tree && table.Place(tree).IsRead)
            .OrderBy(location => location.SourceTree!.FilePath, System.StringComparer.Ordinal)
            .ThenBy(location => location.SourceSpan.Start)
            .FirstOrDefault();
}
