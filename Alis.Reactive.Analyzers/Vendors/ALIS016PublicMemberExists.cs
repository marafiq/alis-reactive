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

    private static readonly DiagnosticDescriptor MissingListening = new(
        Id,
        "A listening member is public on its vendor type",
        "Declaration '{0}' listens through '{1}', which type '{2}' in module '{3}' and its bases do not make public; name a public listening member",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Missing, MissingListening);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            RootMemberDoor.Register(start, Read);
            RootListening.Register(start, ReadListening);
        });
    }

    private static void ReadListening(SymbolAnalysisContext context, RootListening listening)
    {
        if (listening.Member is null)
            context.ReportDiagnostic(Diagnostic.Create(MissingListening, listening.Location, listening.Declaration, listening.Name, listening.Type, listening.Module));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.IsEvent) return;
        if (door.Vendor is null)
            context.ReportDiagnostic(Diagnostic.Create(Missing, door.NameLocation, door.Member, door.Name, door.Type, door.Module));
    }
}
