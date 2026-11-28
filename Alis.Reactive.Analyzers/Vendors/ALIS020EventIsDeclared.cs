using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS020EventIsDeclared : DiagnosticAnalyzer
{
    public const string Id = "ALIS020";

    private static readonly DiagnosticDescriptor Missing = new(
        Id,
        "A declared event belongs to its vendor type",
        "Declaration member '{0}' names event '{1}', which type '{2}' in module '{3}' and its bases do not declare; name an event in the vendor's TypeScript contract",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Missing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start => RootMemberDoor.Register(start, Read));
    }
    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.IsEvent && !door.DeclaredEvent)
            context.ReportDiagnostic(Diagnostic.Create(Missing, door.NameLocation, door.Member, door.Name, door.Type, door.Module));
    }
}
