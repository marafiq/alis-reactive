using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS015MirroredTypeExists : DiagnosticAnalyzer
{
    public const string Id = "ALIS015";

    private static readonly DiagnosticDescriptor Missing = new(
        Id,
        "A mirrored type belongs to the vendor contract",
        "Declaration '{0}' mirrors type '{1}' in module '{2}', which the vendor contract lacks; name a type in the project's TypeScript program",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Ambiguous = new(
        Id,
        "A mirrored type names one vendor declaration",
        "Declaration '{0}' mirrors type '{1}' in module '{2}', which the vendor contract lists more than once; give each TypeScript declaration its own module or name",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Missing, Ambiguous);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var declaration = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.JsObject");
        var mirrors = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.MirrorsAttribute");
        if (declaration is null || mirrors is null) return;
        var table = VendorContract.Read(start.Options, start.CancellationToken);
        if (table is null) return;

        start.RegisterSymbolAction(context =>
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!DerivesFrom(type, declaration)) return;
            var attribute = type.GetAttributes().FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, mirrors));
            if (attribute is null || attribute.AttributeConstructor is null || attribute.ConstructorArguments.Length != 2 || attribute.ConstructorArguments[0].Value is not string module || attribute.ConstructorArguments[1].Value is not string name) return;
            var count = table.Count(module, name);
            if (count == 1) return;
            if (attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) is not AttributeSyntax { ArgumentList: { } arguments }) return;
            var parameter = attribute.AttributeConstructor.Parameters[1];
            var argument = arguments.Arguments.FirstOrDefault(value => value.NameColon?.Name.Identifier.ValueText == parameter.Name);
            if (argument is null && arguments.Arguments.Count > 1 && arguments.Arguments[1].NameColon is null) argument = arguments.Arguments[1];
            if (argument is not null)
                context.ReportDiagnostic(Diagnostic.Create(count == 0 ? Missing : Ambiguous, argument.Expression.GetLocation(), type.Name, name, module));
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
