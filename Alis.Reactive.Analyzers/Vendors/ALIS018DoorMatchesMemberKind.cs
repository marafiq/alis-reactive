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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyOnMethod, NotCallable, CannotWrite, CannotRead);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start => RootMemberDoor.Register(start, Read));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.Vendor is not { } member) return;
        var rule = door.IsMethod
            ? !member.Callable || member.Kind == "writeonly" ? NotCallable : null
            : member.Kind switch
            {
                "method" => PropertyOnMethod,
                "readonly" when door.Door is "Property" or "WriteOnly" => CannotWrite,
                "writeonly" when door.Door is "Property" or "ReadOnly" => CannotRead,
                _ => null,
            };
        if (rule is not null)
            context.ReportDiagnostic(Diagnostic.Create(rule, door.DoorLocation, door.Member, door.Door, door.Name));
    }
}
