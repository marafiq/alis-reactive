using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS016PublicMemberExists : DiagnosticAnalyzer
{
    public const string Id = "ALIS016";

    private static readonly DiagnosticDescriptor Missing = new(
        Id,
        "A declared member is public on its vendor type",
        "Declaration member '{0}' names '{1}', which type '{2}' in module '{3}' and its bases do not make public; name a public member of that type",
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
        if (door.Vendor is null)
            context.ReportDiagnostic(Diagnostic.Create(Missing, door.NameLocation, door.Member, door.Name, door.Type, door.Module));
    }
}
