using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS018DoorMatchesMemberKind : DiagnosticAnalyzer
{
    public const string Id = "ALIS018";

    private static readonly DiagnosticDescriptor PropertyOnMethod = new(
        Id,
        "A declaration door follows its vendor member kind",
        "Declaration member '{0}' uses {1} for vendor method '{2}'; use Method or MethodAsync",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NotCallable = new(
        Id,
        "A declaration door follows its vendor member kind",
        "Declaration member '{0}' uses {1} for vendor member '{2}', which cannot be called; use a property door that matches its access",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor CannotWrite = new(
        Id,
        "A writing door names a writable vendor member",
        "Declaration member '{0}' uses {1} for read-only vendor member '{2}'; use ReadOnly",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor CannotRead = new(
        Id,
        "A reading door names a readable vendor member",
        "Declaration member '{0}' uses {1} for write-only vendor member '{2}'; use WriteOnly",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor CallbackProperty = new(
        Id,
        "A callback names a writable callable property",
        "Declaration member '{0}' uses Callback for vendor member '{2}', which is not a writable callable property; name a writable callable property",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NotCallableListening = new(
        Id,
        "A listening member can be called",
        "Declaration '{0}' listens through '{1}', which cannot be called; name a callable listening member",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NotFunctionRemover = new(
        Id,
        "A returned unsubscriber returns a function",
        "Declaration '{0}' listens through '{1}', whose return is not only a remover function; name a subscribe method that returns a function and nothing else",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NotAssignableEvent = new(
        Id,
        "An assigned event is a writable callable property",
        "Declaration member '{0}' listens by assignment to '{1}', which is not a writable callable property; name a writable callable event property",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor EventAnswers = new(
        Id,
        "An event listener owes its vendor no answer",
        "'{0}' returns {1}, an answer its vendor reads, and a handler answers nothing. Declare it as Callback<TArgs, TResult> and write the answer as an expression.",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyOnMethod, NotCallable, CannotWrite, CannotRead, CallbackProperty, NotCallableListening, NotFunctionRemover, NotAssignableEvent, EventAnswers);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            RootMemberDoor.Register(start, Read);
            RootListening.Register(start, ReadListening, ReadEvent);
        });
    }

    private static void ReadListening(SymbolAnalysisContext context, RootListening listening)
    {
        if (listening.Member is not { } member) return;
        if (!member.Callable || member.Kind == "writeonly")
            context.ReportDiagnostic(Diagnostic.Create(NotCallableListening, listening.Location, listening.Declaration, listening.Name));
        else if (listening.Returned && !member.ReturnsPromise && member.ReturnsNonFunction)
            context.ReportDiagnostic(Diagnostic.Create(NotFunctionRemover, listening.Location, listening.Declaration, listening.Name));
    }

    private static void ReadEvent(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.Vendor is not { Kind: "property", Callable: true } member)
            context.ReportDiagnostic(Diagnostic.Create(NotAssignableEvent, door.DoorLocation, door.Member, door.Name));
        else if (member.EventAnswer is { } answer)
            context.ReportDiagnostic(Diagnostic.Create(EventAnswers, door.DoorLocation, door.Name, answer));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.IsEvent) return;
        if (door.Vendor is not { } member) return;
        var rule = door.Door == "Callback"
            ? member.Kind != "property" || !member.Callable ? CallbackProperty : null
            : door.IsMethod
            ? !member.Callable || member.Kind == "writeonly" ? NotCallable : null
            : member.Kind switch
            {
                "method" => PropertyOnMethod,
                "readonly" when door.Door is "Property" or "WriteOnly" => CannotWrite,
                "writeonly" when door.Door is "Property" or "ReadOnly" or "LiveObject" or "LiveObjects" => CannotRead,
                _ => null,
            };
        if (rule is not null)
            context.ReportDiagnostic(Diagnostic.Create(rule, door.DoorLocation, door.Member, door.Door, door.Name));
    }
}
