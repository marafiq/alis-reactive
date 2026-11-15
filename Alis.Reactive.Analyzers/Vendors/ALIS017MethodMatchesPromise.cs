using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS017MethodMatchesPromise : DiagnosticAnalyzer
{
    public const string Id = "ALIS017";

    private static readonly DiagnosticDescriptor NeedsAsync = new(
        Id,
        "A method door follows its vendor promise",
        "Declaration member '{0}' uses Method for '{1}', whose vendor signatures return a promise; use MethodAsync",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NeedsSync = new(
        Id,
        "A method door follows its vendor promise",
        "Declaration member '{0}' uses MethodAsync for '{1}', whose vendor signatures return no promise; use Method",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NeedsSynchronousObject = new(
        Id,
        "A returned object door requires a synchronous return",
        "Declaration member '{0}' uses {1} for '{2}', whose vendor signatures return a promise; this live-object door requires a synchronous return",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AsyncListening = new(
        Id,
        "A returned unsubscriber is returned synchronously",
        "Declaration '{0}' listens through '{1}', whose signatures return a promise; name a subscribe method that returns its remover synchronously",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NeedsAsync, NeedsSync, NeedsSynchronousObject, AsyncListening);

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
        if (listening.Returned && listening.Member is { Callable: true, ReturnsPromise: true, Kind: not "writeonly" })
            context.ReportDiagnostic(Diagnostic.Create(AsyncListening, listening.Location, listening.Declaration, listening.Name));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (!door.IsMethod || door.Vendor is not { Callable: true, HasSignatures: true } member || member.Kind == "writeonly") return;
        if (member.ReturnsPromise && door.Door == "Method")
            context.ReportDiagnostic(Diagnostic.Create(NeedsAsync, door.DoorLocation, door.Member, door.Name));
        if (member.ReturnsPromise && door.Door is "ReturnedObject" or "ReturnedObjects")
            context.ReportDiagnostic(Diagnostic.Create(NeedsSynchronousObject, door.DoorLocation, door.Member, door.Door, door.Name));
        if (!member.ReturnsPromise && door.Door == "MethodAsync")
            context.ReportDiagnostic(Diagnostic.Create(NeedsSync, door.DoorLocation, door.Member, door.Name));
    }
}
